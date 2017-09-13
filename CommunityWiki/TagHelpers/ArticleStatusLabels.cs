using System.Text;
using CommunityWiki.Models.Articles;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CommunityWiki.TagHelpers
{
    /// <summary>
    /// Displays labels based on the article's status
    /// </summary>
    [HtmlTargetElement("article-status", Attributes = nameof(Article), TagStructure = TagStructure.WithoutEndTag)]
    public class ArticleStatusLabels : TagHelper
    {
        public ArticleModel Article { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var sb = new StringBuilder();

            if (!Article.PublishedOn.HasValue)
                sb.AppendLine("<span class=\"label label-default\" title=\"This article is not published\">Not published</span>");

            if (Article.IsFlaggedForReview)
                sb.AppendLine("<span class=\"label label-warning\" title=\"This article is not published\">Needs review</span>");

            if (Article.IsFlaggedForDeletion)
                sb.Append("<span class=\"label label-danger\" title=\"This article is flagged for review\">Deletion</span>");

            output.Content.SetHtmlContent(sb.ToString());

            output.TagName = "";

        }
    }
}
