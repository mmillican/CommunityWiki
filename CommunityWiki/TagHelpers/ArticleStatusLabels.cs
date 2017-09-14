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
                sb.AppendLine(FormatLabelTag("label-default", "Not published", "This article is not yet published"));

            if (Article.IsFlaggedForReview)
                sb.AppendLine(FormatLabelTag("label-warning", "Needs review", "this article needs review"));

            if (Article.IsFlaggedForDeletion)
                sb.AppendLine(FormatLabelTag("label-danger", "To delete", "This article is marked for deletion"));

            output.Content.SetHtmlContent(sb.ToString());

            output.TagName = "";
        }

        private static string FormatLabelTag(string labelClass, string text, string altText = null)
        {
            if (!labelClass.StartsWith("label-"))
                labelClass = $"label-{labelClass}";

            var sb = new StringBuilder();
            sb.Append($"<span class=\"label {labelClass}\"");

            if (!string.IsNullOrEmpty(altText))
                sb.Append($" title=\"{altText}\"");

            sb.Append($">{text}</span>");

            return sb.ToString();
        }
    }
}
