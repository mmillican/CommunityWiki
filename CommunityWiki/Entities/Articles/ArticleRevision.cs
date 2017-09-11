using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityWiki.Entities.Articles
{
    public class ArticleRevision
    {
        [Key]
        public int Id { get; set; }

        public int ArticleId { get; set; }
        [ForeignKey(nameof(ArticleId))]
        public virtual Article Article { get; set; }

        public DateTime RevisionDate { get; set; }
        public int RevisionUserId { get; set; }

        public string Body { get; set; }
        public string Data { get; set; }

        [MaxLength(255)]
        public string Comment { get; set; }

        public int Score { get; set; }
    }
}
