using Postboy.Enums;

namespace Postboy.Data
{
    public class AutoHeader
    {
        public Guid RequestId { get; set; }
        public AutoHeaderType Type { get; set; }
    }
}
