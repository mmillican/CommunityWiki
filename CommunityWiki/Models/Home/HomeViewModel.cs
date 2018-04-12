using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityWiki.Models.Articles;
using CommunityWiki.Models.ArticleTypes;

namespace CommunityWiki.Models.Home
{
    public class HomeViewModel
    {
        public List<ArticleModel> RecentlyCreated { get; set; } = new List<ArticleModel>();
        public List<ArticleModel> RecentlyUpdated { get; set; } = new List<ArticleModel>();

        public List<TopArticleTypeModel> TopTypes { get; set; } = new List<TopArticleTypeModel>();
    }

    public class TopArticleTypeModel
    {
        public ArticleTypeModel ArticleType { get; set; }

        public int Count { get; set; }
    }
}
