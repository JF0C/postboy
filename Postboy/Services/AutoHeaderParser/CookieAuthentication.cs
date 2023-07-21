namespace Postboy.Services.AutoHeaderParser
{
    public class CookieAuthentication : IAutoHeaderParser
    {
        private const string _name = "Cookie Authentication";
        private static readonly Guid _guid = new ("79501273-42E4-472C-AAD7-B19A33768386");
        public string Name { get => _name; set { } } 

        public Guid Guid { get => _guid; set { } }
        public Task<(string Key, string Value)> ParseHeader(HttpResponseMessage responseMessage)
        {
            var val = responseMessage.Headers.FirstOrDefault(h => h.Key.ToLower() == "set-cookie").Value;
            return Task.FromResult(("Cookie", val?.FirstOrDefault() ?? ""));
        }
    }
}
