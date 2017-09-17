using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityWiki.Entities.Articles
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }
        
        public int ArticleId { get; set; }
        [ForeignKey(nameof(ArticleId))]
        public virtual Article Article { get; set; }

        public DateTime VotedOn { get; set; }

        public int UserId { get; set; }

        public VoteType VoteType { get; set; }
    }

    public enum VoteType
    {
        DownVote = -1,
        UpVote = 1,
        NeedsReview = 10,
        Deletion = 20
    }
}
