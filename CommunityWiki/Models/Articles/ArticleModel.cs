﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityWiki.Models.Articles
{
    public class ArticleModel
    {
        public int Id { get; set; }

        public int ArticleTypeId { get; set; }
        public string ArticleTypeName { get; set; }
        
        public int? ParentId { get; set; }

        public DateTime? PublishedOn { get; set; }

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

        public List<ArticleFieldModel> Fields { get; set; } = new List<ArticleFieldModel>();
    }


    public class ArticleFieldModel
    {
        public int FieldId { get; set; }

        public string Name { get; set; }
        [JsonIgnore]
        public string Description { get; set; }

        public string Value { get; set; }
    }
}
