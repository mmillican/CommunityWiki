using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Config;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Entities.Users;
using CommunityWiki.Helpers;
using CommunityWiki.Models;
using CommunityWiki.Models.Articles;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Models.Votes;
using CommunityWiki.Services;
using DiffPlex.DiffBuilder;
using HeyRed.MarkdownSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CommunityWiki.Controllers
{
    [Authorize(Policy = Constants.Policies.ApprovedUser)]
    [Route("articles")]
    public class ArticlesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISearchService _searchService;
        private readonly IVoteService _voteService;
        private readonly ISideBySideDiffBuilder _diffBuilder;
        private readonly ILogger _logger;
        private readonly ArticleConfig _articleConfig;

        public ArticlesController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext dbContext,
            IMapper mapper,
            IDateTimeService dateTimeService,
            ISearchService searchService,
            IVoteService voteService,
            ISideBySideDiffBuilder diffBuilder,
            ILoggerFactory loggerFactory,
            IOptions<ArticleConfig> articleConfig)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
            _searchService = searchService;
            _voteService = voteService;
            _diffBuilder = diffBuilder;
            _logger = loggerFactory.CreateLogger<ArticlesController>();
            _articleConfig = articleConfig.Value;
        }

        [HttpGet("")]
        [HttpGet("{typeSlug}")]
        public async Task<IActionResult> Index(string typeSlug = null)
        {
            ArticleType type = null;
            if (typeSlug.HasValue())
            {
                type = await _dbContext.ArticleTypes.FirstOrDefaultAsync(x => x.Slug == typeSlug);
            }

            var articles = await _dbContext.Articles
                    .Include(x => x.ArticleType)
                .Where(x => x.DeletedOn == null
                    && (type == null || x.ArticleTypeId == type.Id))
                .ProjectTo<ArticleModel>()
                .ToListAsync();

            var model = new ArticleListViewModel
            {
                Articles = articles,
                ArticlesConfig = _articleConfig
            };

            if (type != null)
            {
                model.Type = Mapper.Map<ArticleTypeModel>(type);
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet("{typeSlug}/{slug}/{id}")]
        public async Task<IActionResult> ViewArticle(int id, string typeSlug, string slug)
        {
            var article = await _dbContext.Articles.Include(x => x.ArticleType).SingleOrDefaultAsync(x => x.Id == id);
            if (article == null || article.DeletedOn.HasValue)
            {
                _logger.LogInformation($"Article ID {id} not found (SLUG: {slug})");
                return RedirectToAction(nameof(Index));
            }

            var user = await GetCurrentUser();

            var markdown = new Markdown();

            var model = _mapper.Map<ArticleViewModel>(article);
            model.IsLoggedIn = _signInManager.IsSignedIn(User);
            model.ArticlesConfig = _articleConfig;

            model.Body = markdown.Transform(model.Body);
            model.Revisions = await BuildArticleRevisionHistory(id);

            if (!string.IsNullOrEmpty(article.PostData))
            {
                model.Fields = JsonConvert.DeserializeObject<List<ArticleFieldModel>>(article.PostData);
            }

            var articleVotes = await _voteService.GetVotesForArticle(id);

            if (user != null)
            {
                model.Voting.UserCanVote = true;
                model.Voting.UserHasUpVoted = articleVotes.Any(x => x.UserId == user.Id && x.VoteType == VoteType.UpVote);
                model.Voting.UserHasDownVoted = articleVotes.Any(x => x.UserId == user.Id && x.VoteType == VoteType.DownVote);
                model.Voting.UpVoteCount = articleVotes.Count(x => x.VoteType == VoteType.UpVote);
                model.Voting.DownVoteCount = articleVotes.Count(x => x.VoteType == VoteType.DownVote);
            }

            return View(model);
        }

        [HttpGet("{articleId}/compare/{revisionId}")]
        public async Task<IActionResult> CompareRevision(int articleId, int revisionId)
        {
            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                _logger.LogInformation($"COMPARE: Article ID {articleId} not found");
                return RedirectToAction(nameof(Index));
            }

            var revision = await _dbContext.ArticleRevisions.FindAsync(revisionId);
            if (revision == null || revision.ArticleId != articleId)
            {
                _logger.LogInformation($"COMPARE: Revision ID {revisionId} / Article ID {articleId} not found");
                return RedirectToAction(nameof(Index));
            }

            var model = new CompareRevisionViewModel();
            model.Article = _mapper.Map<ArticleModel>(article);
            model.Revision = _mapper.Map<ArticleRevisionModel>(revision);
            
            model.DiffModel = _diffBuilder.BuildDiffModel(revision.Body, article.Body);

            return View(model);
        }

        [HttpGet("new")]
        public async Task<IActionResult> Create(int typeId)
        {
            var type = await _dbContext.ArticleTypes.FindAsync(typeId);
            if (type == null)
            {
                // TODO: show invalid type message
                return RedirectToAction(nameof(Index));
            }

            var model = new EditArticleViewModel
            {
                ArticleTypeId = type.Id,
                ArticleTypeName = type.Name
            };
            
            model.Fields = await _dbContext.ArticleTypeFieldDefinitions
                .Where(x => x.ArticleTypeId == typeId)
                .OrderBy(x => x.Order)
                .Select(x => new ArticleFieldModel
                {
                    FieldId = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    FieldType = x.FieldType
                })
                .ToListAsync();

            return View(model);
        }

        [HttpPost("new")]
        public async Task<IActionResult> Create(EditArticleViewModel model)
        {
            var type = await _dbContext.ArticleTypes.FindAsync(model.ArticleTypeId);
            model.ArticleTypeName = type.Name;

            var user = await GetCurrentUser();

            try
            {
                var article = new Article
                {
                    ArticleTypeId = model.ArticleTypeId,
                    Title = model.Title,
                    Slug = model.Title.Slugify(),
                    Body = model.Body,
                    CreatedOn = _dateTimeService.GetNowUtc(),
                    CreatedUserId = user.Id,
                    UpdatedOn = _dateTimeService.GetNowUtc(),
                    UpdatedUserId = user.Id
                };

                article.PostData = JsonConvert.SerializeObject(model.Fields);

                _dbContext.Articles.Add(article);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Article created. ID: {article.Id} / Title: {article.Title}");                

                //try
                //{
                //    await _searchService.IndexArticle(article);
                //}
                //catch(Exception ex)
                //{
                //    _logger.LogError(ex, $"Error indexing article ID {article.Id}");
                //}

                var redirUrl = Url.ViewArticleLink(article.Id, type.Slug, article.Slug);
                return Redirect(redirUrl);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating article", model);

                ModelState.AddModelError("", "Error creating article");
                return View(model);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null || article.DeletedOn.HasValue)
            {
                _logger.LogInformation($"EDIT ARTICLE: Article ID {id} not found");
                return RedirectToAction(nameof(Index));
            }

            var model = _mapper.Map<EditArticleViewModel>(article);
            
            model.Fields = await _dbContext.ArticleTypeFieldDefinitions
                .Where(x => x.ArticleTypeId == article.ArticleTypeId)
                .OrderBy(x => x.Order)
                .Select(x => new ArticleFieldModel
                {
                    FieldId = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    FieldType = x.FieldType
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(article.PostData))
            {
                var fieldData = JsonConvert.DeserializeObject<List<ArticleFieldModel>>(article.PostData);
                foreach (var field in model.Fields)
                {
                    field.Value = fieldData.SingleOrDefault(fv => fv.FieldId == field.FieldId)?.Value;
                }
            }

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, EditArticleViewModel model)
        {
            var user = await GetCurrentUser();

            try
            {
                var article = await _dbContext.Articles.FindAsync(id);
                var articleType = await _dbContext.ArticleTypes.FindAsync(article.ArticleTypeId);

                var origTitle = article.Title;
                article.Title = model.Title;
                if (article.Title != origTitle)
                    article.Slug = article.Title.Slugify();

                var origBody = article.Body;
                article.Body = model.Body;

                article.PostData = JsonConvert.SerializeObject(model.Fields);

                article.UpdatedOn = _dateTimeService.GetNowUtc();
                article.UpdatedUserId = user.Id;

                article.RevisionCount++;

                var revision = new ArticleRevision
                {
                    ArticleId = id,
                    RevisionDate = _dateTimeService.GetNowUtc(),
                    Body = origBody,
                    Score = article.Score,
                    RevisionUserId = user.Id
                };

                _dbContext.ArticleRevisions.Add(revision);

                await _dbContext.SaveChangesAsync();

                //try
                //{
                //    await _searchService.IndexArticle(article);
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, $"Error indexing article ID {article.Id}");
                //}
                
                return Redirect(Url.ViewArticleLink(id, articleType.Slug, article.Slug));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating article ID {id}", model);

                ModelState.AddModelError("", "Error updating article");
                return View(model);
            }
        }

        [HttpGet("review/{queue=publish}")]
        public async Task<IActionResult> Review(string queue)
        {
            IQueryable<Article> articleQuery = _dbContext.Articles;

            switch (queue)
            {
                case "publish":
                    articleQuery = articleQuery.Where(x => !x.PublishedOn.HasValue).OrderBy(x => x.UpdatedOn);
                    break;
            }

            var model = new ReviewQueueViewModel();
            model.Queue = queue;
            model.Articles = await articleQuery.ProjectTo<ArticleModel>().ToListAsync();

            return View(model);
        }

        [HttpPost("{articleId}/publish")]
        public async Task<IActionResult> Publish(int articleId)
        {
            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound();

            if (article.PublishedOn.HasValue)
                return BadRequest("Article is already published");

            // TODO: auth user
            var user = await GetCurrentUser();

            try
            {
                article.PublishedOn = _dateTimeService.GetNowUtc();
                article.PublishedUserId = user.Id;

                await _dbContext.SaveChangesAsync();

                var model = _mapper.Map<ArticleModel>(article);
                return Ok(model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error publishing article {articleId}");

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error publishing article");
            }
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null || article.DeletedOn.HasValue)
                return NotFound();
            
            try
            {
                var user = await GetCurrentUser();

                article.DeletedOn = _dateTimeService.GetNowUtc();
                article.DeletedUserId = user.Id;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting article {id}");

                // TODO: show more friendly error
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error deleting article");
            }
        }

        private async Task<List<ArticleRevisionModel>> BuildArticleRevisionHistory(int articleId)
        {
            var result = new List<ArticleRevisionModel>();

            try
            {
                // TODO: query is pulling too many columns back for users

                var revisions = await _dbContext.ArticleRevisions
                    .Join(_dbContext.Users, x => x.RevisionUserId, x => x.Id, (Revision, User) =>
                        new { Revision, UserFirstName = User.FirstName, UserLastName = User.LastName })
                    .Where(x => x.Revision.ArticleId == articleId)
                    .OrderByDescending(x => x.Revision.RevisionDate)
                    .ToListAsync();

                result = revisions.Select(x =>
                {
                    var rev = _mapper.Map<ArticleRevisionModel>(x.Revision);
                    rev.RevisionUserName = $"{x.UserFirstName} {x.UserLastName}";
                    return rev;
                }).ToList();

                // TODO: compare against previous revision

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error building article revision history for article ID {articleId}");
            }


            return result;
        }

        private async Task<User> GetCurrentUser() => await _userManager.GetUserAsync(User);
    }
}