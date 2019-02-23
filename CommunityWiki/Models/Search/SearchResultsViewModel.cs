using CommunityWiki.Models.Articles;
using System.Collections.Generic;

namespace CommunityWiki.Models.Search
{
    public class SearchResultsViewModel
    {
        public string SearchTerm { get; set; }

        public int ResultCount { get; set; }

        public List<ArticleModel> Articles { get; set; } = new List<ArticleModel>();
    }
}
