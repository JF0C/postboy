using Postboy.Data;

namespace Postboy.Helpers
{
    public class AppStateInitializer
    {
        public static AppState InitializeAppState()
        {
            var appState = new AppState();
            appState.RootFolder = new Folder()
            {
                Name = "Folders"
            };

            return appState;
        }
    }
}
