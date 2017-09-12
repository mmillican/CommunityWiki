using System;
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
        private readonly ILogger _logger;

        public ArticlesController(UserManager<User> userManager,
            ApplicationDbContext dbContext,
            IDateTimeService dateTimeService,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _dateTimeService = dateTimeService;
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

            var model = article.ToViewModel();
            //model.ArticleType = 


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

        private async Task<User> GetCurrentUser() => await _userManager.GetUserAsync(User);
    }
}