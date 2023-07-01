using Postboy.Data;
using Postboy.Data.ContentTypes;
using Postboy.DTOs;
using Postboy.Enums;
using Postboy.Helpers;
using System.Net;
using System.Text.Json;

namespace Postboy.Services
{
    public class RequestExecutorService : IRequestExecutorService
    {
        private readonly IRequestStorageService _storage;
        private readonly IComponentInteractionService _intercom;
        public RequestExecutorService(IRequestStorageService storage, IComponentInteractionService intercom)
        {
            _storage = storage;
            _intercom = intercom;
        }
        public async Task<HttpResponseMessage> Execute(StoredRequest request, bool evaluateAutoHeaders = true)
        {
            try
            {
                if (evaluateAutoHeaders)
                {
                    SetStatus("Starting");
                }
                var cookieContainer = new CookieContainer();
                var clientHander = new HttpClientHandler();
                clientHander.CookieContainer = cookieContainer;
                var client = new HttpClient(clientHander) { BaseAddress = new Uri(request.Url) };
                var httpRequest = new HttpRequestMessage(request.Method, "");
                foreach (var item in request.Headers)
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
                if (evaluateAutoHeaders)
                {
                    SetStatus($"Executing: {request.Name}");
                }
                if (request.ContentType is ContentTypeJson)
                {
                    httpRequest.Content = JsonContent.Create(request.Body ?? "");
                }
                else if (request.ContentType is ContentTypeFormEncoded)
                {
                    httpRequest.Content = ContentConversion.StringToFormContent(request.Body);
                }
                var response = await client.SendAsync(httpRequest);
                var cookies = cookieContainer.GetCookies(new Uri(request.Url));
                if (cookies is not null && cookies.Count > 0)
                {
                    response.Headers.Add("Set-Cookie", cookies.Aggregate("", (a, b) => a + b + ";").TrimEnd(';'));
                }
                return response;

            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($" Request execution failed: {ex.Message}")
                };
            }
            finally
            {
                SetStatus(string.Empty);
            }
        }

        private async Task <(string key, string value)?> EvaluateAutoHeader(AutoHeader autoHeader)
        {
            SetStatus($"Loading: {autoHeader.Type}");
            var request = await _storage.GetById(autoHeader.RequestId);
            if (request == null)
            {
                return null;
            }
            var response = await Execute(request, false);
            return await ParseHeaderValue(autoHeader.Type, response);
        }

        private async Task<(string, string)> ParseHeaderValue(AutoHeaderType type, HttpResponseMessage message)
        {
            switch (type)
            {
                case AutoHeaderType.None:
                    return (string.Empty, string.Empty);
                case AutoHeaderType.BearerAuthentication:
                    var content = await message.Content.ReadAsStringAsync();
                    var token = JsonSerializer.Deserialize<TokenResponseDto>(content);
                    return ("Authorization", $"Bearer {token?.AccessToken}");
                case AutoHeaderType.CookieAuthentication:
                    var val = message.Headers.FirstOrDefault(h => h.Key.ToLower() == "set-cookie").Value;
                    return ("Cookie", val?.FirstOrDefault() ?? "");
                default: 
                    throw new NotImplementedException();
            }
        }

        private void SetStatus(string status)
        {
            _intercom.Notify("RequestExecutionStatus", status);
        }
    }
}
