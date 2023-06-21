using System.Web;

namespace Postboy.Helpers
{
    public static class ContentConversion
    {
        public static FormUrlEncodedContent StringToFormContent(string content)
        {
            return new FormUrlEncodedContent(StringToKeyValuePairs(content).ToArray());
        }

        public static List<KeyValuePair<string, string>> StringToKeyValuePairs (string content)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(content);
            var keyValuePairs = new List<KeyValuePair<string, string>>();
            foreach (string key in nameValueCollection)
            {
                keyValuePairs.Add(new KeyValuePair<string, string>(key, nameValueCollection[key]));
            }
            return keyValuePairs;
        }

        public static string KeyValuePairsToUrlEncodedString(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var result = string.Empty;
            foreach(var kv in keyValuePairs)
            {
                result += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
            }
            result.TrimEnd('&');
            return result;
        }
    }
}
