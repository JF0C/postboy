using Postboy.Data;
using Postboy.Data.ContentTypes;
using Postboy.DTOs;
using Postboy.Enums;
using Postboy.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Postboy.Services
{
    public class RequestExecutorService : IRequestExecutorService
    {
        private readonly IRequestStorageService _storage;
        public RequestExecutorService(IRequestStorageService storage) { _storage = storage; }
        public async Task<HttpResponseMessage> Execute(StoredRequest request, bool evaluateAutoHeaders = true)
        {
            var client = new HttpClient() { BaseAddress = new Uri(request.Url) };
            var httpRequest = new HttpRequestMessage(request.Method, "");
            foreach(var item in request.Headers)
            {
                httpRequest.Headers.Add(item.Key, item.Value);
            }
            if (evaluateAutoHeaders)
            {
                foreach (var autoHeader in request.AutoHeaders)
                {
                    var kv = await EvaluateAutoHeader(autoHeader);
                    if (kv is not null)
                    {
                        httpRequest.Headers.Add(kv.Value.key, kv.Value.value);
                    }
                }
            }
            if (request.ContentType is ContentTypeJson)
            {
                httpRequest.Content = JsonContent.Create(request.Body ?? "");
            }
            else if (request.ContentType is ContentTypeFormEncoded)
            {
                httpRequest.Content = ContentConversion.StringToFormContent(request.Body);
            }
            return await client.SendAsync(httpRequest);
        }

        private async Task <(string key, string value)?> EvaluateAutoHeader(AutoHeader autoHeader)
        {
            var request = await _storage.GetById(autoHeader.RequestId);
            if (request == null)
            {
                return null;
            }
            var response = await Execute(request, false);
            var value = await ParseHeaderValue(autoHeader.Type, response);
            return (autoHeader.Key, value);
        }

        private async Task<string> ParseHeaderValue(AutoHeaderType type, HttpResponseMessage message)
        {
            switch (type)
            {
                case AutoHeaderType.None:
                    return string.Empty;
                case AutoHeaderType.BearerAuthentication:
                    var content = await message.Content.ReadAsStringAsync();
                    var token = JsonSerializer.Deserialize<TokenResponseDto>(content);
                    return $"Bearer {token?.AccessToken}";
                case AutoHeaderType.WidowsFormsAuthentication:
                    throw new NotImplementedException();
                default: 
                    throw new NotImplementedException();
            }
        }
    }
}
