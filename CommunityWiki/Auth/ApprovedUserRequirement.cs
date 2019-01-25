using CommunityWiki.Config;
using CommunityWiki.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace CommunityWiki.Auth
{
    public class ApprovedUserRequirement : IAuthorizationRequirement
    {

    }

    public class ApprovedUserAuthHandler : AuthorizationHandler<ApprovedUserRequirement>
    {
        private readonly UserManager<User> _userManager;
        private readonly UserConfig _userConfig;

        public ApprovedUserAuthHandler(UserManager<User> userManager,
            IOptions<UserConfig> userConfig)
        {
            _userManager = userManager;
            _userConfig = userConfig.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ApprovedUserRequirement requirement)
        {
            if (!_userConfig.RequireNewUserApproval)
            {
                context.Succeed(requirement);
            }

            var user = await _userManager.GetUserAsync(context.User);
            if (user?.IsApproved ?? false)
            {
                context.Succeed(requirement);
            }
        }
    }

}
