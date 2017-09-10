using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CommunityWiki.Entities.Users
{
    public class User : IdentityUser<int>
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }
    }
}
