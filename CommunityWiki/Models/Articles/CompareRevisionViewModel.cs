using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder.Model;

namespace CommunityWiki.Models.Articles
{
    public class CompareRevisionViewModel
    {
        public ArticleModel Article { get; set; }
        public ArticleRevisionModel Revision { get; set; }

        public SideBySideDiffModel DiffModel { get; set; }
    }
}
