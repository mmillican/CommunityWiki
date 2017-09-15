using System.Collections.Generic;

namespace CommunityWiki.Models.Articles
{
    public class ReviewQueueViewModel
    {
        public string Queue { get; set; }

        public List<ArticleModel> Articles { get; set; }
    }
}
