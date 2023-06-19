using Postboy.Data;

namespace Postboy.Services
{
    public class RequestExecutorService : IRequestExecutorService
    {
        public async Task<HttpResponseMessage> Execute(StoredRequest request)
        {
            var client = new HttpClient() { BaseAddress = new Uri(request.Url) };
            var httpRequest = new HttpRequestMessage(request.Method, "");
            foreach(var item in request.Headers)
            {
                httpRequest.Headers.Add(item.Key, item.Value);
            }
            httpRequest.Content = new StringContent(request.Body ?? "");
            return await client.SendAsync(httpRequest);
        }
    }
}
