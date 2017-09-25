using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityWiki.Config
{
    public class SearchConfig
    {
        public string ElasticNodeUri { get; set; }
        public string IndexName { get; set; }
    }
}
