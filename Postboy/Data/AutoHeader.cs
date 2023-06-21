using Postboy.Enums;

namespace Postboy.Data
{
    public class AutoHeader
    {
        public string Key { get; set; } = string.Empty;
        public Guid RequestId { get; set; }
        public AutoHeaderType Type { get; set; }
    }
}
