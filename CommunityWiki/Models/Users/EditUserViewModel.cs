using System.ComponentModel.DataAnnotations;

namespace CommunityWiki.Models.Users
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email address")]
        [Required, MaxLength(255)]
        public string Email { get; set; }

        [Display(Name = "First name")]
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public bool IsAdmin { get; set; }
    }
}
