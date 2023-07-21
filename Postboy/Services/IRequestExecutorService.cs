using Microsoft.AspNetCore.Components;
using Postboy.Data;
using Postboy.Services.AutoHeaderParser;

namespace Postboy.Services
{
    public interface IRequestExecutorService
    {
        Task<HttpResponseMessage> Execute(StoredRequest request, bool evaluateAutoHeaders = true);
        List<IAutoHeaderParser> AutoHeaders { get; set; }
    }
}
