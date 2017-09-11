using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityWiki.Entities.Articles
{
    public class ArticleType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public virtual List<FieldDefinition> Fields { get; set; }

        public ArticleType()
        {
            Fields = new List<FieldDefinition>();
        }
    }
}
