using Postboy.Data;
namespace Postboy.Services
{
    public interface IRequestStorageService
    {
        Task<List<StoredRequest>> GetAll();
        Task<StoredRequest?> GetById(Guid id);
        Task<StoredRequest?> GetByName(string name);
        Task<bool> Delete(Guid id);
        Task<bool> Create(StoredRequest request);
        Task<bool> Update(StoredRequest request);

        Task<Folder> GetFolders();
        Task<Folder> CreateFolder(Guid parent, string name);
        Task<bool> AddRequestToFolder(Guid folderId, Guid requestId);
        Task<bool> DeleteFolder(Guid folderId);
        Task<bool> RenameFolder(Guid folderId, string name);
        Task<bool> RemoveRequestFromFolder(Guid folderId, Guid requestId);
    }
}
