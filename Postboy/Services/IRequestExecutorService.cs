using Microsoft.AspNetCore.Components;
using Postboy.Data;

namespace Postboy.Services
{
    public interface IRequestExecutorService
    {
        Task<HttpResponseMessage> Execute(StoredRequest request, bool evaluateAutoHeaders = true);
    }
}
