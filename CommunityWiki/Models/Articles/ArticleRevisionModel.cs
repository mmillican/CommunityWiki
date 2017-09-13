using System;

namespace CommunityWiki.Models.Articles
{
    public class ArticleRevisionModel
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public DateTime RevisionDate { get; set; }
        public int RevisionUserId { get; set; }
        public string RevisionUserName { get; set; }

        public string Body { get; set; }
        public string Data { get; set; }
        
        public string Comment { get; set; }

        public int Score { get; set; }
    }
}
