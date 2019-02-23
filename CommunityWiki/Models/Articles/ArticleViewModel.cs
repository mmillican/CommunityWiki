using CommunityWiki.Config;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.ArticleTypes;
using System.Collections.Generic;

namespace CommunityWiki.Models.Articles
{
    public class ArticleViewModel : ArticleModel
    {
        public bool IsUserApproved { get; set; }

        public ArticleTypeModel ArticleType { get; set; }

        public List<ArticleRevisionModel> Revisions { get; set; }

        public ArticleVotingModel Voting { get; set; } = new ArticleVotingModel();

        public FlagArticleViewModel Flagging { get; set; } = new FlagArticleViewModel();

        public ArticleConfig ArticlesConfig { get; set; }
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

    public class FlagArticleViewModel
    {
        public int ArticleId { get; set; }
        public VoteType Type { get; set; }
    }
}
