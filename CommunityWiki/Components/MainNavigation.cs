using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Data;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Models.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityWiki.Components
{
    public class MainNavigation : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public MainNavigation(ApplicationDbContext dbContext,
            IAuthorizationService authorizationService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new MainNavigationViewModel();
            model.IsAdmin = (await _authorizationService.AuthorizeAsync(UserClaimsPrincipal, Constants.Policies.Admin)).Succeeded;

            model.ArticleTypes = await _dbContext.ArticleTypes
                .OrderBy(x => x.Name)
                .ProjectTo<ArticleTypeModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return View(model);
        }
    }
}
