using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using Microsoft.EntityFrameworkCore;

namespace CommunityWiki.Services
{
    public interface IVoteService
    {
        Task<Vote> GetVote(int id);
        Task<Vote> GetUserVoteForArticle(int userId, int articleId, VoteType type);

        Task<IList<Vote>> GetUserVotesForArticle(int userId, int articleId);
        Task<IList<Vote>> GetVotesForArticle(int articleId);
        Task<int> GetVoteCountForArticle(int articleId, VoteType? type = null);
        Task UpdateArticleScore(int articleId);

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

        public async Task<Vote> GetVote(int id)
        {
            var vote = await _dbContext.Votes.FindAsync(id);
            return vote;
        }

        public async Task<IList<Vote>> GetUserVotesForArticle(int userId, int articleId)
        {
            var votes = await _dbContext.Votes
                .Where(x => x.UserId == userId && x.ArticleId == articleId)
                .OrderByDescending(x => x.VotedOn)
                .ToListAsync();
            return votes;
        }

        public async Task<IList<Vote>> GetVotesForArticle(int articleId)
        {
            var votes = await _dbContext.Votes
                .Where(x => x.ArticleId == articleId)
                .OrderByDescending(x => x.VotedOn)
                .ToListAsync();
            return votes;
        }

        public async Task<Vote> GetUserVoteForArticle(int userId, int articleId, VoteType type)
        {
            var vote = await _dbContext.Votes.FirstOrDefaultAsync(x =>
                x.UserId == userId
                && x.ArticleId == articleId
                && x.VoteType == type);

            return vote;
        }

        public async Task<int> GetVoteCountForArticle(int articleId, VoteType? type = null)
        {
            var voteCount = await _dbContext.Votes.Where(x =>
                x.ArticleId == articleId
                && (type == null || x.VoteType == type))
                .CountAsync();

            return voteCount;
        }

        public async Task UpdateArticleScore(int articleId)
        {
            try
            {
                var article = await _dbContext.Articles.FindAsync(articleId);
                var votes = await GetVotesForArticle(articleId);
                var upVotes = votes.Where(x => x.VoteType == VoteType.UpVote).Count();
                var downVotes = votes.Where(x => x.VoteType == VoteType.DownVote).Count();

                article.Score = upVotes - downVotes;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating article score", ex);
            }
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

            await UpdateArticleScore(vote.ArticleId);

            return vote;
        }

        private bool DoesTypeAllowMultipleVotes(VoteType type)
        {
            return _allowsMultipleVotesTypes.Contains(type);
        }
    }
}
