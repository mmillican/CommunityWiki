using System;
using System.Linq;
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
        public async Task GetUserVotesForArticle_NoVotesForArticle_ReturnsEmpty()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 2, UserId = 2 });
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

                var vote = await voteService.GetUserVoteForArticle(1, 1, VoteType.UpVote);
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task GetUserVoteForArticle_NoVoteOfType_ReturnsNull()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetUserVoteForArticle(2, 1, VoteType.DownVote);
                Assert.Null(vote);
            }
        }

        [Fact]
        public async Task GetUserVoteForArticle_Found_ReturnsVote()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = await voteService.GetUserVoteForArticle(2, 1, VoteType.UpVote);
                Assert.NotNull(vote);
                Assert.Equal(1, vote.Id);
                Assert.Equal(1, vote.ArticleId);
                Assert.Equal(2, vote.UserId);
                Assert.Equal(VoteType.UpVote, vote.VoteType);
            }
        }

        [Fact]
        public async Task GetVoteCountForArticle_NoTypeSpecified_NoVotes_ReturnsZero()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var voteCount = await voteService.GetVoteCountForArticle(1, null);
                Assert.NotNull(voteCount);
                Assert.Equal(0, voteCount);
            }
        }

        [Fact]
        public async Task GetVoteCountForArticle_TypeSpecified_NoVotes_ReturnsZero()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var voteCount = await voteService.GetVoteCountForArticle(1, VoteType.UpVote);
                Assert.NotNull(voteCount);
                Assert.Equal(0, voteCount);
            }
        }

        [Fact]
        public async Task GetVoteCountForArticle_NoVotesForType_ReturnsZero()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var voteCount = await voteService.GetVoteCountForArticle(1, VoteType.DownVote);
                Assert.NotNull(voteCount);
                Assert.Equal(0, voteCount);
            }
        }

        [Fact]
        public async Task GetVoteCountForArticle_MultipleVoteTypes_ReturnsCount()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                testContext.Votes.Add(new Vote { Id = 2, ArticleId = 1, UserId = 3, VoteType = VoteType.DownVote });
                testContext.Votes.Add(new Vote { Id = 3, ArticleId = 1, UserId = 4, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var voteCount = await voteService.GetVoteCountForArticle(1, VoteType.UpVote);
                Assert.NotNull(voteCount);
                Assert.Equal(2, voteCount);

                voteCount = await voteService.GetVoteCountForArticle(1, VoteType.DownVote);
                Assert.NotNull(voteCount);
                Assert.Equal(1, voteCount);
            }
        }

        [Fact]
        public async Task UpdateArticleScore_NoVotes_EqualsZero()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);
                
                await voteService.UpdateArticleScore(1);

                var updatedArticle = await testContext.Articles.FindAsync(1);
                Assert.NotNull(updatedArticle);
                Assert.Equal(0, updatedArticle.Score);
            }
        }

        [Fact]
        public async Task UpdateArticleScore_SingleUpVote_EqualsOne()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                await voteService.UpdateArticleScore(1);

                var updatedArticle = await testContext.Articles.FindAsync(1);
                Assert.NotNull(updatedArticle);
                Assert.Equal(1, updatedArticle.Score);
            }
        }

        [Fact]
        public async Task UpdateArticleScore_SingleDownVote_EqualsNegOne()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.DownVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                await voteService.UpdateArticleScore(1);

                var updatedArticle = await testContext.Articles.FindAsync(1);
                Assert.NotNull(updatedArticle);
                Assert.Equal(-1, updatedArticle.Score);
            }
        }
        
        [Fact]
        public async Task UpdateArticleScore_OneUpAndDownVote_EqualsZero()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                testContext.Votes.Add(new Vote { Id = 2, ArticleId = 1, UserId = 5, VoteType = VoteType.DownVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                await voteService.UpdateArticleScore(1);

                var updatedArticle = await testContext.Articles.FindAsync(1);
                Assert.NotNull(updatedArticle);
                Assert.Equal(0, updatedArticle.Score);
            }
        }

        [Fact]
        public async Task UpdateArticleScore_ThreeUpAndDownVote_EqualsTwo()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 2, VoteType = VoteType.UpVote });
                testContext.Votes.Add(new Vote { Id = 2, ArticleId = 1, UserId = 4, VoteType = VoteType.DownVote });
                testContext.Votes.Add(new Vote { Id = 4, ArticleId = 1, UserId = 6, VoteType = VoteType.UpVote });
                testContext.Votes.Add(new Vote { Id = 6, ArticleId = 1, UserId = 7, VoteType = VoteType.UpVote });

                // Add a vote from another article
                testContext.Votes.Add(new Vote { Id = 10, ArticleId = 2, UserId = 2, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                await voteService.UpdateArticleScore(1);

                var updatedArticle = await testContext.Articles.FindAsync(1);
                Assert.NotNull(updatedArticle);
                Assert.Equal(2, updatedArticle.Score);
            }
        }
        
        [Fact]
        public async Task CreateVote_NullVote_ThrowsException()
        {
            using (var testContext = GetTestDbContext())
            {
                var voteService = new VoteService(testContext);
                
                await Assert.ThrowsAsync<ArgumentNullException>(() => voteService.CreateVote(null));
            }
        }

        [Fact]
        public async Task CreateVote_ArticleNotFound_ThrowsException()
        {
            using (var testContext = GetTestDbContext())
            {
                var voteService = new VoteService(testContext);

                var vote = new Vote
                {
                    ArticleId = 1
                };

                await Assert.ThrowsAsync<ApplicationException>(() => voteService.CreateVote(vote));
            }
        }


        [Fact]
        public async Task CreateVote_VoteTypeDoesNotAllowMultiple_UserVoted_ThrowsException()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 1, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = new Vote
                {
                    Id = 2,
                    ArticleId = 1,
                    UserId = 1,
                    VoteType = VoteType.UpVote
                };

                await Assert.ThrowsAsync<ApplicationException>(() => voteService.CreateVote(vote));                
            }
        }

        [Fact]
        public async Task CreateVote_VoteTypeDoesNotAllowMultiple_UserDidNotVote_CreatesVote()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = new Vote
                {
                    Id = 1,
                    ArticleId = 1,
                    UserId = 1,
                    VoteType = VoteType.UpVote
                };

                await voteService.CreateVote(vote);

                Assert.Equal(1, testContext.Votes.Count());

                var savedVote = await testContext.Votes.FindAsync(vote.Id);
                Assert.NotNull(savedVote);

                var article = await testContext.Articles.FindAsync(vote.ArticleId);
                Assert.NotNull(article);
                Assert.Equal(1, article.Score);
            }
        }

        [Fact]
        public async Task CreateVote_VoteTypeAllowsMulitple_UserVoted_CreatesVote()
        {
            using (var testContext = GetTestDbContext())
            {
                testContext.Articles.Add(new Article { Id = 1, Title = "Test Article", Slug = "test-article" });
                testContext.Votes.Add(new Vote { Id = 1, ArticleId = 1, UserId = 1, VoteType = VoteType.UpVote });
                await testContext.SaveChangesAsync();

                var voteService = new VoteService(testContext);

                var vote = new Vote
                {
                    Id = 2,
                    ArticleId = 1,
                    UserId = 1,
                    VoteType = VoteType.NeedsReview
                };

                await voteService.CreateVote(vote);

                Assert.Equal(2, testContext.Votes.Count());

                var savedVote = await testContext.Votes.FindAsync(vote.Id);
                Assert.NotNull(savedVote);

                var article = await testContext.Articles.FindAsync(vote.ArticleId);
                Assert.NotNull(article);
                Assert.Equal(1, article.Score); // Score should not change
            }
        }

        private ApplicationDbContext GetTestDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .add
                .Options;

            var testContext = new ApplicationDbContext(dbOptions);

            return testContext;
        }
    }
}
