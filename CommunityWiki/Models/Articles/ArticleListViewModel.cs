using System.Collections.Generic;
using CommunityWiki.Models.ArticleTypes;

namespace CommunityWiki.Models.Articles
{
    public class ArticleListViewModel
    {
        public ArticleTypeModel Type { get; set; }

        public List<ArticleModel> Articles { get; set; }
    }
}
