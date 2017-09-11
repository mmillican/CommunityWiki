using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityWiki.Entities.Articles
{
    public class FieldDefinition
    {
        [Key]
        public int Id { get; set; }

        public int ArticleTypeId { get; set; }
        [ForeignKey(nameof(ArticleTypeId))]
        public virtual ArticleType ArticleType { get; set; }

        public FieldType FieldType { get; set; }

        public int Order { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public int? MaxLength { get; set; }
        public bool IsRequired { get; set; }
    }

    public enum FieldType
    {
        TextBox = 1,
        MultiLineText = 2
    }
}
