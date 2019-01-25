using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommunityWiki.Models;
using CommunityWiki.Data;
using CommunityWiki.Models.Home;
using Microsoft.EntityFrameworkCore;
using CommunityWiki.Models.Articles;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using CommunityWiki.Models.ArticleTypes;

namespace CommunityWiki.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public HomeController(ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();

            var topTypes = _dbContext.Articles
                .Where(x => x.PublishedOn.HasValue
                    && !x.DeletedOn.HasValue)
                .GroupBy(x => x.ArticleType)
                .OrderByDescending(x => x.Count())
                .Take(5)
                .Select(x => new TopArticleTypeModel
                {
                    ArticleType = _mapper.Map<ArticleTypeModel>(x.Key),
                    Count = x.Count()
                });

            var recentlyCreatedArticles = _dbContext.Articles
                .Where(x => x.PublishedOn.HasValue
                    && !x.DeletedOn.HasValue)
                .OrderByDescending(x => x.CreatedOn)
                .Take(10)
                .ProjectTo<ArticleModel>();

            var recentlyUpdatedArticles = _dbContext.Articles
                .Where(x => x.PublishedOn.HasValue
                    && !x.DeletedOn.HasValue)
                .OrderByDescending(x => x.UpdatedOn)
                .Take(10)
                .ProjectTo<ArticleModel>();

            model.TopTypes = await topTypes.ToListAsync();
            model.RecentlyCreated = await recentlyCreatedArticles.ToListAsync();
            model.RecentlyUpdated = await recentlyUpdatedArticles.ToListAsync();

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
