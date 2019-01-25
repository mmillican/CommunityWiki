using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CommunityWiki.Entities.Users
{
    public class User : IdentityUser<int>
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [NotMapped]

        public string FullName { get { return $"{FirstName} {LastName}"; } }

        public DateTime JoinedOn { get; set; }

        [NotMapped]
        public bool IsApproved => ApprovedOn.HasValue;

        public DateTime? ApprovedOn { get; set; }
    }
}
