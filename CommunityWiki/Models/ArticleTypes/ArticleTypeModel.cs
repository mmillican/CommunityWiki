using System.ComponentModel.DataAnnotations;

namespace CommunityWiki.Models.ArticleTypes
{
    public class ArticleTypeModel
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
    }
}
