﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Models.Votes;

namespace CommunityWiki.Models.Articles
{
    public class ArticleViewModel : ArticleModel
    {
        public ArticleTypeModel ArticleType { get; set; }

        public List<ArticleRevisionModel> Revisions { get; set; }

        public ArticleVotingModel Voting { get; set; } = new ArticleVotingModel();
    }

    public class ArticleVotingModel
    {
        public bool UserCanVote { get; set; }
        public bool UserHasUpVoted { get; set; }
        public bool UserHasDownVoted { get; set; }

        public bool UserHasVoted { get => UserHasDownVoted || UserHasUpVoted; }

        public int UpVoteCount { get; set; }
        public int DownVoteCount { get; set; }
    }
}
