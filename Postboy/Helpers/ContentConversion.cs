using System.Web;

namespace Postboy.Helpers
{
    public static class ContentConversion
    {
        public static FormUrlEncodedContent StringToFormContent(string content)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(content);
            var keyValuePairs = new List<KeyValuePair<string, string>>();
            foreach (string key in nameValueCollection)
            {
                keyValuePairs.Add(new KeyValuePair<string, string>(key, nameValueCollection[key]));
            }
            return new FormUrlEncodedContent(keyValuePairs.ToArray());
        }
    }
}
