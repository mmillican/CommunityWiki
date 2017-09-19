using System.Text.RegularExpressions;
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
    }
}
