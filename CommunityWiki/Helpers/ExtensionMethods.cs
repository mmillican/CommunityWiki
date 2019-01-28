using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CommunityWiki.Helpers
{
    public static class ExtensionMethods
    {
        public static string Slugify(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var regex = new Regex("[^a-z0-9\\-_]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            input = input.Replace(" ", "-");
            var cleaned = regex.Replace(input, "").ToLower();

            while (cleaned.Contains("--"))
            {
                cleaned = cleaned.Replace("--", "-");
            }

            return cleaned;
        }

        public static string Truncate(this string input, int maxLength, string append = null)
        {
            if (input.IsNullOrEmpty()) return input;
            if (input.Length <= maxLength) return input;

            if (append.HasValue())
            {
                maxLength = maxLength - append.Length;
            }

            var result = input.Substring(0, maxLength);
            if (append.HasValue())
                result = result + append;

            return result;
        }

        public static string Serialize(this object data, bool camelCaseProps = true)
        {
            if (camelCaseProps)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                return JsonConvert.SerializeObject(data, settings);
            }

            return JsonConvert.SerializeObject(data);
        }

        public static string ToJsBoolean(this bool val) => val.ToString().ToLower();

        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);

        public static bool HasValue(this string input) => !IsNullOrEmpty(input);


        public const string TitleViewDataKey = "Title";        
        public static void SetPageTitle(this ViewDataDictionary viewData, string title) => viewData[TitleViewDataKey] = title;
        public static string GetPageTitle(this ViewDataDictionary viewData) => viewData[TitleViewDataKey] as string;

        public const string SubTitleViewDataKey = "SubTitle";
        public static void SetSubTitle(this ViewDataDictionary viewData, string subTitle) => viewData[SubTitleViewDataKey] = subTitle;
        public static string GetPageSubTitle(this ViewDataDictionary viewData) => viewData[SubTitleViewDataKey] as string;
    }
}
