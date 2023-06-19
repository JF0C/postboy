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
    }
}
