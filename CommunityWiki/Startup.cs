using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommunityWiki.Data;
using CommunityWiki.Entities.Users;
using CommunityWiki.Services;
using AutoMapper;
using System.Reflection;
using DiffPlex.DiffBuilder;
using DiffPlex;
using CommunityWiki.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using CommunityWiki.Auth;

namespace CommunityWiki
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, Role>(config =>
            {
                config.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(Constants.Policies.Admin, policy => policy.RequireRole(Constants.Roles.Admin));
                auth.AddPolicy(Constants.Policies.ApprovedUser, policy => policy.AddRequirements(new ApprovedUserRequirement()));
            });

            var appAssembly = typeof(Startup).GetTypeInfo().Assembly;
            services.AddAutoMapper(appAssembly);

            services.Configure<SearchConfig>(Configuration.GetSection("Search"));
            services.Configure<UserConfig>(Configuration.GetSection("Users"));
            services.Configure<ArticleConfig>(Configuration.GetSection("Articles"));

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IVoteService, VoteService>();
            services.AddTransient<IDiffer, Differ>();
            services.AddTransient<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddScoped<IAuthorizationHandler, ApprovedUserAuthHandler>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddLogging(builder => builder
                .AddConfiguration(Configuration)
                .AddConsole());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
