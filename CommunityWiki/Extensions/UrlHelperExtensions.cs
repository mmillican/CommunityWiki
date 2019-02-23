using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityWiki.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, int userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, int userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ViewArticleLink(this IUrlHelper urlHelper, int articleId, string typeSlug, string slug)
        {
            return urlHelper.Action(nameof(ArticlesController.ViewArticle), "Articles", new { id = articleId, typeSlug, slug });
        }
    }
}
