using System.Collections.Generic;
using CommunityWiki.Entities.Articles;

namespace CommunityWiki.Models.Search
{
    public class SearchResultsViewModel
    {
        public string SearchTerm { get; set; }

        public int ResultCount { get; set; }

        public List<Article> Articles { get; set; } = new List<Article>();
    }
}
