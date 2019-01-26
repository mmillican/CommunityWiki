using System;

namespace CommunityWiki.Models.Users
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public DateTime JoinedOn { get; set; }

        public bool IsApproved => ApprovedOn.HasValue;

        public DateTime? ApprovedOn { get; set; }
    }
}
