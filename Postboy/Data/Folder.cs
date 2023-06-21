using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

namespace Postboy.Data
{
    public class Folder
    {
        public string Name { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Folder> Folders { get; set; } = new();
        public List<Guid> RequestIds { get; set; } = new();
        public bool IsOpen { get; set; }
        [JsonIgnore]
        public Folder? Parent { get; set; }
        [JsonIgnore]
        public string Path => $"{Parent?.Path}/{Name}";
    }
}
