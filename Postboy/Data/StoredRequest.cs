using Postboy.Data.ContentTypes;
using System.Text.Json.Serialization;

namespace Postboy.Data
{
    public class StoredRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        [JsonIgnore]
        public StoredRequestContentType ContentType { get; set; } = new ContentTypeNone();
        public string ContentTypeString { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public List<AutoHeader> AutoHeaders { get; set; } = new();
    }
}
