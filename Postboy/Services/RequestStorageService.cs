using Postboy.Data;
using Postboy.Helpers;
using System.Text.Json;

namespace Postboy.Services
{
    public class RequestStorageService : IRequestStorageService
    {
        private AppState? _appState;
        private string AppStateFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).TrimEnd(Path.DirectorySeparatorChar);
        private string AppStateFile => $"{AppStateFolder}{Path.DirectorySeparatorChar}Postboy{Path.DirectorySeparatorChar}appstate.json";
        public RequestStorageService()
        {
            if (!Directory.Exists(AppStateFolder))
            {
                Directory.CreateDirectory(AppStateFolder);
            }
            if (!File.Exists(AppStateFile))
            {
                _appState = AppStateInitializer.InitializeAppState();
                WriteState(_appState).GetAwaiter().GetResult();
            }
        }

        private async Task<AppState> ReadState(bool force = false)
        {
            if (_appState is null || force)
            {
                using var file = File.OpenRead(AppStateFile);
                using var fileStream = new StreamReader(file);
                var state = JsonSerializer.Deserialize<AppState>(await fileStream.ReadToEndAsync());
                foreach (var r in state.Requests)
                {
                    r.ContentType = StoredRequestContentType.Deserialize(r.ContentTypeString);
                }
                fileStream.Close();
                file.Close();
                if (state is null)
                {
                    throw new Exception("appstate.json invalid");
                }
                _appState = state;
            }
            return _appState;
        }

        private async Task WriteState(AppState state)
        {
            await File.WriteAllTextAsync(AppStateFile, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
            _appState = await ReadState(true);
        }

        public async Task<bool> Create(StoredRequest request)
        {
            var state = await ReadState();
            if (state.Requests.Any(r => r.Name == request.Name))
            {
                throw new Exception($"Request with name {request.Name} already exists");
            }
            state.Requests.Add(request);
            await WriteState(state);
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var state = await ReadState();
            var current = state.Requests.FirstOrDefault(r => r.Id == id);
            if (current is null)
            {
                throw new Exception($"Request {id} not found");
            }
            state.Requests.Remove(current);
            await WriteState(state);
            return true;
        }

        public async Task<List<StoredRequest>> GetAll()
        {
            return (await ReadState()).Requests;
        }

        public async Task<StoredRequest?> GetById(Guid id)
        {
            return (await ReadState()).Requests.FirstOrDefault(r => r.Id == id);
        }

        public async Task<StoredRequest?> GetByName(string name)
        {
            return (await ReadState()).Requests.FirstOrDefault(r => r.Name == name);
        }

        public async Task<bool> Update(StoredRequest request)
        {
            var state = await ReadState();
            var current = state.Requests.FirstOrDefault(r => r.Id == request.Id);
            if (current is null)
            {
                throw new Exception($"Request {request.Name} not found");
            }
            if (state.Requests.Where(r => r.Id != request.Id).Any(r => r.Name == request.Name))
            {
                throw new Exception($"Request with the same name \"{request.Name}\" already exists");
            }
            current.Body = request.Body;
            current.Name = request.Name;
            current.Url = request.Url;
            current.Method = request.Method;
            current.Headers = request.Headers;
            current.AutoHeaders = request.AutoHeaders;
            current.ContentTypeString = StoredRequestContentType.Serialize(request.ContentType);
            await WriteState(state);
            return true;
        }

        public async Task<Folder> GetFolders()
        {
            return (await ReadState()).RootFolder;
        }

        public async Task<Folder> CreateFolder(Guid parent, string name)
        {
            var state = await ReadState();
            var folder = new Folder { Name = name };
            var parentFolder = FindFolderRecursively(state.RootFolder, parent);
            if (parentFolder is null)
            {
                throw new Exception($"Folder {parent} not found!");
            }
            parentFolder.Folders.Add(folder);
            await WriteState(state);
            return folder;
        }

        public async Task<bool> AddRequestToFolder(Guid folderId, Guid requestId)
        {
            var state = await ReadState();
            var parent = FindFolderRecursively(state.RootFolder, folderId);
            var request = state.Requests.FirstOrDefault(r => r.Id == requestId);
            if (parent is null)
            {
                throw new Exception($"Folder {folderId} not found!");
            }
            if (request is null)
            {
                throw new Exception($"Request {requestId} not found!");
            }
            if (parent.RequestIds.Any(r => r == requestId))
            {
                throw new Exception($"Request {request.Name} is already in folder {parent.Name}");
            }
            parent.RequestIds.Add(requestId);
            await WriteState(state);
            return true;
        }

        public async Task<bool> DeleteFolder(Guid folderId)
        {
            var state = await ReadState();
            var folder = FindFolderRecursively(state.RootFolder, folderId);
            var parent = FindParentFolderRecursively(state.RootFolder, folderId);
            if (folder is null)
            {
                throw new Exception($"Cannot find folder {folderId}");
            }
            if (parent is null)
            {
                throw new Exception($"Parent of folder {folderId} cannot be found");
            }
            parent.Folders.Remove(folder);
            await WriteState(state);
            return true;
        }

        public async Task<bool> RenameFolder(Guid folderId, string name)
        {
            var state = await ReadState();
            var folder = FindFolderRecursively(state.RootFolder, folderId);
            if (folder is null)
            {
                throw new Exception($"Folder {folderId} not found!");
            }
            folder.Name = name;
            await WriteState(state);
            return true;
        }

        public async Task<bool> RemoveRequestFromFolder(Guid folderId, Guid requestId)
        {
            var state = await ReadState();
            var folder = FindFolderRecursively(state.RootFolder, folderId);
            if (folder is null)
            {
                throw new Exception($"Folder {folderId} not found!");
            }
            if (folder.RequestIds.Any(id => id == requestId))
            {
                folder.RequestIds.Remove(requestId);
                await WriteState(state);
                return true;
            }
            throw new Exception($"Folder {folderId} does not contain the request {requestId}");
        }

        public async Task<bool> SaveFolders(Folder root)
        {
            var state = await ReadState();
            state.RootFolder = root;
            await WriteState(state);
            return true;
        }

        private Folder? FindFolderRecursively(Folder root, Guid id)
        {
            if (root.Id == id) return root;
            foreach(var folder in root.Folders)
            {
                var result = FindFolderRecursively(folder, id);
                if (result is not null) return result;
            }
            return null;
        }
        private Folder? FindParentFolderRecursively(Folder root, Guid id)
        {
            if (root.Folders.Any(f => f.Id == id) || root.RequestIds.Any(r => r == id)) return root;
            foreach(var folder in root.Folders)
            {
                var result = FindParentFolderRecursively(folder, id);
                if (result is not null) return result;
            }
            return null;
        }

        public async Task<List<Folder>> GetFoldersFlat()
        {
            var state = await ReadState();
            var result = new List<Folder>();
            return AggregateFolders(result, state.RootFolder);
        }

        private List<Folder> AggregateFolders(List<Folder> folders, Folder current)
        {
            folders.AddRange(current.Folders);
            foreach(var folder in current.Folders)
            {
                folder.Parent = current;
                folders = AggregateFolders(folders, folder);
            }
            return folders;
        }
    }
}
