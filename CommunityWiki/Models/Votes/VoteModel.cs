using System;
using AutoMapper;
using CommunityWiki.Entities.Articles;

namespace CommunityWiki.Models.Votes
{
    public class VoteModel
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }

        public DateTime VotedOn { get; set; }

        public int UserId { get; set; }

        public VoteType VoteType { get; set; }
    }

    public static partial class ModelExtensions
    {
        public static VoteModel ToModel(this Vote vote) => Mapper.Map<VoteModel>(vote);
    }
}
