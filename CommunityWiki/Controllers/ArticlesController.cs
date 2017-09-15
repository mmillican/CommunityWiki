using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Entities.Users;
using CommunityWiki.Helpers;
using CommunityWiki.Models;
using CommunityWiki.Models.Articles;
using CommunityWiki.Services;
using DiffPlex.DiffBuilder;
using HeyRed.MarkdownSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityWiki.Controllers
{
    [Route("articles")]
    public class ArticlesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISideBySideDiffBuilder _diffBuilder;
        private readonly ILogger _logger;

        public ArticlesController(UserManager<User> userManager,
            ApplicationDbContext dbContext,
            IDateTimeService dateTimeService,
            ISideBySideDiffBuilder diffBuilder,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _dateTimeService = dateTimeService;
            _diffBuilder = diffBuilder;
            _logger = loggerFactory.CreateLogger<ArticlesController>();
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var articles = await _dbContext.Articles
                    .Include(x => x.ArticleType)
                .Where(x => x.DeletedOn == null)
                .ProjectTo<ArticleModel>()
                .ToListAsync();

            var model = new ArticleListViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [HttpGet("{id}/{slug}")]
        public async Task<IActionResult> ViewArticle(int id, string slug)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
            {
                _logger.LogInformation($"Article ID {id} not found (SLUG: {slug})");
                return RedirectToAction(nameof(Index));
            }

            var markdown = new Markdown();

            var model = article.ToViewModel();

            model.Body = markdown.Transform(model.Body);
            model.Revisions = await BuildArticleRevisionHistory(id);

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
            model.Article = article.ToModel();
            model.Revision = revision.ToModel();
            
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

                _dbContext.Articles.Add(article);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Article created. ID: {article.Id} / Title: {article.Title}");

                var redirUrl = Url.ViewArticleLink(article.Id, article.Slug);
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
            if (article == null)
            {
                _logger.LogInformation($"EDIT ARTICLE: Article ID {id} not found");
                return RedirectToAction(nameof(Index));
            }

            var model = article.ToEditModel();

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, EditArticleViewModel model)
        {
            var user = await GetCurrentUser();

            try
            {
                var article = await _dbContext.Articles.FindAsync(id);

                var origTitle = article.Title;
                article.Title = model.Title;
                if (article.Title != origTitle)
                    article.Slug = article.Title.Slugify();

                var origBody = article.Body;
                article.Body = model.Body;
                
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

                return Redirect(Url.ViewArticleLink(id, article.Slug));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating article ID {id}", model);

                ModelState.AddModelError("", "Error updating article");
                return View(model);
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
                    var rev = x.Revision.ToModel();
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