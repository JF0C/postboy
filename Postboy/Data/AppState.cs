namespace Postboy.Data
{
    public class AppState
    {
        public List<StoredRequest> Requests { get; set; } = new();
        public Folder RootFolder { get; set; } = new();
    }
}
