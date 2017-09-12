using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityWiki.Models.ArticleTypes;

namespace CommunityWiki.Models.Articles
{
    public class ArticleViewModel : ArticleModel
    {
        public ArticleTypeModel ArticleType { get; set; }
    }
}
