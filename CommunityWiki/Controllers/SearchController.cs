using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.Articles;
using CommunityWiki.Models.Search;
using CommunityWiki.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;

namespace CommunityWiki.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SearchController(ISearchService searchService,
            ApplicationDbContext dbContext,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _searchService = searchService;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<SearchController>();

        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string query = "")
        {
            var model = new SearchResultsViewModel
            {
                SearchTerm = query
            };

            try
            {
                model.Articles = await ExecuteSqlSearch(query);
                model.ResultCount = model.Articles.Count;

                //var searchRequest = new SearchRequest<Article>
                //{                    
                //    MinScore = 0.2, // TODO: verify this is a "good" value (along with tie breaker and must match %-ages
                //    Query = new BoolQuery
                //    {
                //        Should = new QueryContainer[]
                //        {
                //            new MultiMatchQuery
                //            {
                //                Fields = new[] { "title"},
                //                MinimumShouldMatch = new MinimumShouldMatch("30%"),
                //                TieBreaker = 0.2,
                //                Type = TextQueryType.PhrasePrefix,
                //                Query = query,
                //                Boost = 10
                //            },
                //            new MultiMatchQuery
                //            {
                //                Fields = new[] { "body"},
                //                MinimumShouldMatch = new MinimumShouldMatch("30%"),
                //                TieBreaker = 0.2,
                //                Type = TextQueryType.PhrasePrefix,
                //                Query = query,
                //                Boost = 4
                //            }
                //        }
                //    }
                //};

                //var response = await _searchService.Client.SearchAsync<Article>(searchRequest);
                //if (response.Hits.Count > 0)
                //{
                //    model.ResultCount = response.Hits.Count;

                //    foreach(var hit in response.Hits)
                //    {
                //        model.Articles.Add(hit.Source);
                //    }
                //}

                return View(model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error executing search for '{query}'");
            }

            return View(model);
        }

        private async Task<List<ArticleModel>> ExecuteSqlSearch(string query)
        {
            var articles = await _dbContext.Articles.Where(x => x.PublishedOn.HasValue
                && x.Title.Contains(query)
                || x.Body.Contains(query)
                || x.PostData.Contains(query))
            .ProjectTo<ArticleModel>(_mapper.ConfigurationProvider)
            .ToListAsync();

            return articles;
        }
    }
}