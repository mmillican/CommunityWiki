using System;
using System.Net;
using System.Threading.Tasks;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Entities.Users;
using CommunityWiki.Models.Votes;
using CommunityWiki.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommunityWiki.Controllers
{
    [Route("votes")]
    public class VotesController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IVoteService _voteService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _logger;
        
        public VotesController(UserManager<User> userManager,
            IVoteService voteService,
            IDateTimeService dateTimeService,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _voteService = voteService;
            _dateTimeService = dateTimeService;
            _logger = loggerFactory.CreateLogger<VotesController>();
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(VoteModel model)
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

                model = vote.ToModel();
                return Created("", model);
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

        private async Task<User> GetCurrentUser() => await _userManager.GetUserAsync(User);
    }
}
