namespace Postboy.Services.AutoHeaderParser
{
    public class CookieAuthentication : IAutoHeaderParser
    {
        private const string _name = "Cookie Authentication";
        private static readonly Guid _guid = new ("79501273-42E4-472C-AAD7-B19A33768386");
        public string Name { get => _name; set { } } 

        public Guid Guid { get => _guid; set { } }
        public Task<(string Key, string Value, DateTime Expiration)> ParseHeader(HttpResponseMessage responseMessage)
        {
            var val = responseMessage.Headers.FirstOrDefault(h => h.Key.ToLower() == "set-cookie").Value.FirstOrDefault();
            if (val == null)
            {
                return Task.FromResult(("Cookie", "", DateTime.Now));
            }
            var components = val.Split(';');
            var value = "";
            var expiration = DateTime.Now;
            foreach(var c in components)
            {
                if (c.StartsWith("expires="))
                {
                    expiration = DateTime.Parse(c.Replace("expires=", ""));
                }
                else
                {
                    value = c;
                }
            }
            return Task.FromResult(("Cookie", value, expiration));
        }
    }
}
