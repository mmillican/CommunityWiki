using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CommunityWiki.Entities.Users;
using Microsoft.AspNetCore.Identity;
using CommunityWiki.Entities.Articles;

namespace CommunityWiki.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<FieldDefinition> ArticleTypeFieldDefinitions { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleRevision> ArticleRevisions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        }
    }
}
