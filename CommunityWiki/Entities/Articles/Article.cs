using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityWiki.Entities.Articles
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        public int ArticleTypeId { get; set; }
        [ForeignKey(nameof(ArticleTypeId))]
        public virtual ArticleType ArticleType { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public virtual Article Parent { get; set; }

        public DateTime? PublishedOn { get; set; }
        public int? PublishedUserId { get; set; }
        
        [Required, MaxLength(500)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        public string Slug { get; set; }

        [Required]
        public string Body { get; set; }

        public int Score { get; set; }
        public int ViewCount { get; set; }
        public int RevisionCount { get; set; }

        public string PostData { get; set; }

        public bool IsFlaggedForReview { get; set; }
        public bool IsFlaggedForDeletion { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedUserId { get; set; }
        public DateTime? DeletedOn { get; set; }
        public int? DeletedUserId { get; set; }
    }
}
