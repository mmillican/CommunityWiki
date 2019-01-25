using System;
using System.Net;
using System.Threading.Tasks;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Entities.Users;
using CommunityWiki.Models.Votes;
using CommunityWiki.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommunityWiki.Controllers
{
    [Authorize(Policy = Constants.Policies.ApprovedUser)]
    [Route("votes")]
    [Produces("application/json")]
    public class VotesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IVoteService _voteService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _logger;
        
        public VotesController(UserManager<User> userManager,
            ApplicationDbContext dbContext,
            IVoteService voteService,
            IDateTimeService dateTimeService,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _voteService = voteService;
            _dateTimeService = dateTimeService;
            _logger = loggerFactory.CreateLogger<VotesController>();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] VoteModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var user = await GetCurrentUser();

                var vote = new Vote
                {
                    ArticleId = model.ArticleId,
                    VotedOn = _dateTimeService.GetNowUtc(),
                    UserId = user.Id,
                    VoteType = model.VoteType
                };

                vote = await _voteService.CreateVote(vote);

                var artVoteCount = await GetVoteCountForArticle(model.ArticleId, model.VoteType);
                var score = (await _dbContext.Articles.FindAsync(model.ArticleId))?.Score;

                var voteResultModel = new
                {
                    model.ArticleId,
                    model.VoteType,
                    VoteCount = artVoteCount,
                    Score = score
                };

                return Created("", voteResultModel);
            }
            catch (ApplicationException ex) when (ex.Message == "Article does not exist")
            {
                _logger.LogError(ex, $"Article {model.ArticleId} does not exist");
                return NotFound("Article not found");
            }
            catch (ApplicationException ex) when (ex.Message.Contains("does not allow multiple votes"))
            {
                var errorMsg = "Vote type does not allow multiple votes";
                _logger.LogInformation(errorMsg);
                return StatusCode((int)HttpStatusCode.BadRequest, errorMsg);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating vote", model.ArticleId, model);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error creating vote");
            }
        }

        [HttpDelete("")]
        public async Task<IActionResult> Delete([FromBody] VoteModel model)
        {
            try
            {
                var user = await GetCurrentUser();

                Vote vote;
                if (model.Id != 0)
                    vote = await _voteService.GetVote(model.Id);
                else
                    vote = await _voteService.GetUserVoteForArticle(user.Id, model.ArticleId, model.VoteType);

                if (vote == null)
                {
                    _logger.LogInformation("Vote not found");
                    return NotFound("Vote not found");
                }

                _dbContext.Votes.Remove(vote);
                await _dbContext.SaveChangesAsync();
                await _voteService.UpdateArticleScore(model.ArticleId);

                var artVoteCount = await GetVoteCountForArticle(model.ArticleId, model.VoteType);
                var score = (await _dbContext.Articles.FindAsync(model.ArticleId))?.Score;

                var voteResultModel = new
                {
                    model.ArticleId,
                    model.VoteType,
                    VoteCount = artVoteCount,
                    Score = score
                };

                return Ok(voteResultModel);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting vote", model);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error deleting vote");
            }
        }

        private async Task<int> GetVoteCountForArticle(int articleId, VoteType type)
        {
            var count = await _voteService.GetVoteCountForArticle(articleId, type);
            return count;
        }

        private async Task<User> GetCurrentUser() => await _userManager.GetUserAsync(User);
    }
}
