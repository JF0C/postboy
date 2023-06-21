using Postboy.Data;
using System.Text.Json;

namespace Postboy.Services
{
    public class RequestStorageService : IRequestStorageService
    {
        public RequestStorageService()
        {

        }

        private async Task<AppState> ReadState()
        {
            using var file = File.OpenRead("appstate.json");
            using var fileStream = new StreamReader(file);
            var state = JsonSerializer.Deserialize<AppState>(await fileStream.ReadToEndAsync());
            foreach(var r in state.Requests)
            {
                r.ContentType = StoredRequestContentType.Deserialize(r.ContentTypeString);
            }
            fileStream.Close();
            file.Close();
            if (state is null)
            {
                throw new Exception("appstate.json invalid");
            }
            return state;
        }

        private async Task WriteState(AppState state)
        {
            await File.WriteAllTextAsync("appstate.json", JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
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
    }
}
