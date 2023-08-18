namespace Postboy.Services.AutoHeaderParser
{
    public interface IAutoHeaderParser
    {
        public string Name { get; set; }

        public Guid Guid { get; set; }

        public Task<(string Key, string Value, DateTime Expiration)> ParseHeader(HttpResponseMessage responseMessage);
    }
}
