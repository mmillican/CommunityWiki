using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Config;
using CommunityWiki.Entities.Users;
using CommunityWiki.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityWiki.Controllers
{
    [Authorize(Policy = Constants.Policies.Admin)]
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ISystemClock _systemClock;
        private readonly UserConfig _userConfig;
        private readonly ILogger<UsersController> _logger;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public UsersController(UserManager<User> userManager,
            IOptions<UserConfig> userConfig,
            IMapper mapper,
            ISystemClock systemClock,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _systemClock = systemClock;
            _userConfig = userConfig.Value;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = new UserListViewModel();
            model.Users = await _userManager.Users
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ProjectTo<UserModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return View(model);
        }

        // This should be a post, but for UI purposes, we want a link for now
        [HttpGet("{userId}/approve")]
        public async Task<IActionResult> Approve(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                ErrorMessage = "The requested user was not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                user.ApprovedOn = _systemClock.UtcNow.DateTime;

                await _userManager.UpdateAsync(user);

                SuccessMessage = "The user has been approved.";
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error approving user {userId}", userId);
                ErrorMessage = "There was an error approving the user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                ErrorMessage = "The requested user was not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = _mapper.Map<EditUserViewModel>(user);
            model.IsAdmin = await _userManager.IsInRoleAsync(user, Constants.Roles.Admin);

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                ErrorMessage = "The requested user was not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                await _userManager.UpdateAsync(user);

                if (model.IsAdmin && !await _userManager.IsInRoleAsync(user, Constants.Roles.Admin))
                {
                    await _userManager.AddToRoleAsync(user, Constants.Roles.Admin);
                }
                else if (!model.IsAdmin && await _userManager.IsInRoleAsync(user, Constants.Roles.Admin))
                {
                    await _userManager.RemoveFromRoleAsync(user, Constants.Roles.Admin);
                }

                SuccessMessage = "The user has been updated.";
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating user {userId}", id);
                ErrorMessage = "There was an error updating the user. Please try again.";
                return View(model);
            }

        }
    }
}