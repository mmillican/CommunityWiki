using CommunityWiki.Entities.Articles;
using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace CommunityWiki.Models.ArticleTypes
{
    public class FieldDefinitionModel
    {
        public int Id { get; set; }

        public int ArticleTypeId { get; set; }
        public FieldType FieldType { get; set; }
        public string FieldTypeName => FieldType.Humanize();

        public int Order { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public int? MaxLength { get; set; }
        public bool IsRequired { get; set; }
    }
}
