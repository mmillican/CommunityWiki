using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommunityWiki.Tests.Services
{
    
    public class VoteServiceTests
    {
        [Fact]
        public async Task GetVote_NoVotes_ReturnsNull()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetVote(1);
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task GetVote_InvalidVoteId_ReturnsNull()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetVote(2);
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task GetVote_ValidVoteId_ReturnsVote()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetVote(1);
                Assert.NotNull(vote);
                Assert.Equal(1, vote.Id);
                Assert.Equal(1, vote.ArticleId);
                Assert.Equal(2, vote.UserId);
            }
        }

        [Fact]
        public async Task GetUserVotesForArticle_NoVotes_ReturnsEmpty()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var votes = await voteService.GetUserVotesForArticle(1, 1);
                Assert.NotNull(votes);
                Assert.Equal(0, votes.Count);
            }
        }

        [Fact]
        public async Task GetUserVotesForArticle_NoVotesForUser_ReturnsEmpty()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2 });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var votes = await voteService.GetUserVotesForArticle(1, 1);
                Assert.NotNull(votes);
                Assert.Equal(0, votes.Count);
            }
        }

        [Fact]
        public async Task GetUserVotesForArticle_VotesForUser_ReturnsVotes()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 1 });
                testContext.Votes.Add(new Vote { Id = 2, ArticleId = 1, UserId = 2 });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var votes = await voteService.GetUserVotesForArticle(1, 1);
                Assert.NotNull(votes);
                Assert.Equal(1, votes.Count);

                var firstTestVote = votes[0];
                Assert.NotNull(firstTestVote);
                Assert.Equal(1, firstTestVote.Id);
                Assert.Equal(1, firstTestVote.ArticleId);
                Assert.Equal(1, firstTestVote.UserId);
            }
        }
        
        [Fact]
        public async Task GetUserVoteForArticle_NoVotesForArticle_ReturnsNull()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetUserVoteForArticle(It.IsAny<int>(), 1, It.IsAny<VoteType>());
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task GetUserVoteForArticle_NoVotesForUser_ReturnsNull()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetUserVoteForArticle(2, 1, VoteType.UpVote);
                Assert.Null(vote);
            }
        }

        private ApplicationDbContext GetTestDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var testContext = new ApplicationDbContext(dbOptions);

            return testContext;
        }
    }
}
