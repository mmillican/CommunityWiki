using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Entities.Users;
using CommunityWiki.Models;
using CommunityWiki.Models.ArticleTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityWiki.Controllers
{
    [Authorize] // TODO: Admin only
    [Route("article-types")]
    public class ArticleTypesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ArticleTypesController(UserManager<User> userManager,
            ApplicationDbContext dbContext,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<ArticleTypesController>();
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var types = await _dbContext.ArticleTypes
                .OrderBy(x => x.Name)
                .ProjectTo<ArticleTypeModel>()
                .ToListAsync();

            var model = new ArticleTypeListViewModel
            {
                ArticleTypes = types
            };

            return View(model);
        }

        [HttpGet("{id}")]
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet("new")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EditArticleTypeViewModel model)
        {
            try
            {
                var type = new ArticleType
                {
                    Name = model.Name,
                    Description = model.Description
                };

                _dbContext.ArticleTypes.Add(type);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Edit), new { id = type.Id });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating article type", model);

                ModelState.AddModelError("", "Error adding new article type");
                return View(model);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _dbContext.ArticleTypes.FindAsync(id);
            if (type == null)
                return RedirectToAction(nameof(Index));

            var model = type.ToEditModel();

            return View(model);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditArticleTypeViewModel model)
        {
            try
            {
                var type = await _dbContext.ArticleTypes.FindAsync(id);
                if (type == null)
                    return RedirectToAction(nameof(Index));

                type.Name = model.Name;
                type.Description = model.Description;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article type", id, model);

                ModelState.AddModelError("", "Error updating article type");
                return View(model);
            }
        }
    }
}