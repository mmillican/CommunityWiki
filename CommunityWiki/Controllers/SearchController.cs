using System;
using System.Threading.Tasks;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.Search;
using CommunityWiki.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;

namespace CommunityWiki.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger _logger;

        public SearchController(ISearchService searchService,
            ILoggerFactory loggerFactory)
        {
            _searchService = searchService;
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
                var searchRequest = new SearchRequest<Article>
                {                    
                    MinScore = 0.2, // TODO: verify this is a "good" value (along with tie breaker and must match %-ages
                    Query = new BoolQuery
                    {
                        Should = new QueryContainer[]
                        {
                            new MultiMatchQuery
                            {
                                Fields = new[] { "title"},
                                MinimumShouldMatch = new MinimumShouldMatch("30%"),
                                TieBreaker = 0.2,
                                Type = TextQueryType.PhrasePrefix,
                                Query = query,
                                Boost = 10
                            },
                            new MultiMatchQuery
                            {
                                Fields = new[] { "body"},
                                MinimumShouldMatch = new MinimumShouldMatch("30%"),
                                TieBreaker = 0.2,
                                Type = TextQueryType.PhrasePrefix,
                                Query = query,
                                Boost = 4
                            }
                        }
                    }
                };

                var response = await _searchService.Client.SearchAsync<Article>(searchRequest);
                if (response.Hits.Count > 0)
                {
                    model.ResultCount = response.Hits.Count;

                    foreach(var hit in response.Hits)
                    {
                        model.Articles.Add(hit.Source);
                    }
                }

                return View(model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error executing search for '{query}'");
            }

            return View(model);
        }
    }
}