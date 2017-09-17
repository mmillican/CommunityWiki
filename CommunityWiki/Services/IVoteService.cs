using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using Microsoft.EntityFrameworkCore;

namespace CommunityWiki.Services
{
    public interface IVoteService
    {
        Task<Vote> CreateVote(Vote vote);
    }

    public class VoteService : IVoteService
    {
        private static readonly List<VoteType> _allowsMultipleVotesTypes = new List<VoteType> {
            VoteType.NeedsReview,
            VoteType.Deletion
        };

        private readonly ApplicationDbContext _dbContext;

        public VoteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Vote> GetUserVoteForArticle(int userId, int articleId, VoteType type)
        {
            var vote = await _dbContext.Votes.FirstOrDefaultAsync(x =>
                x.UserId == userId
                && x.ArticleId == userId
                && x.VoteType == type);

            return vote;
        }

        public async Task<Vote> CreateVote(Vote vote)
        {
            if (vote == null)
                throw new ArgumentNullException(nameof(vote));

            var article = await _dbContext.Articles.FindAsync(vote.ArticleId);
            if (article == null)
                throw new ApplicationException("Article does not exist");

            if (!DoesTypeAllowMultipleVotes(vote.VoteType))
            {
                var existingVote = await GetUserVoteForArticle(vote.UserId, vote.ArticleId, vote.VoteType);
                if (existingVote != null)
                {
                    throw new ApplicationException($"Vote type '{vote.VoteType}' does not allow multiple votes");
                }
            }

            _dbContext.Votes.Add(vote);
            await _dbContext.SaveChangesAsync();

            return vote;
        }

        private bool DoesTypeAllowMultipleVotes(VoteType type)
        {
            return _allowsMultipleVotesTypes.Contains(type);
        }
    }
}
