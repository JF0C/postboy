using Postboy.Data;

namespace Postboy.Services
{
    public interface IRequestExecutorService
    {
        Task<HttpResponseMessage> Execute(StoredRequest request);
    }
}
