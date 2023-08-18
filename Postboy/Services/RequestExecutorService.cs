using Postboy.Data;
using Postboy.Data.ContentTypes;
using Postboy.Helpers;
using Postboy.Services.AutoHeaderParser;
using System.Net;
using System.Text.Json;

namespace Postboy.Services
{
    public class RequestExecutorService : IRequestExecutorService
    {
        private record CachedAuthHeader (string Key, string Value, DateTime Expires);

        private readonly IRequestStorageService _storage;
        private readonly IComponentInteractionService _intercom;
        private readonly List<IAutoHeaderParser> _autoHeaders;
        private readonly Dictionary<Guid, CachedAuthHeader> _cachedHeaders = new();
        public RequestExecutorService(IRequestStorageService storage, IComponentInteractionService intercom)
        {
            _storage = storage;
            _intercom = intercom;
            _autoHeaders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IAutoHeaderParser).IsAssignableFrom(p) && p.IsClass)
                .Select(t => Activator.CreateInstance(t) as IAutoHeaderParser)
                .Where(p => p is not null)
                .ToList()!;
        }
        public async Task<HttpResponseMessage> Execute(StoredRequest request)
        {
            return await ExecuteInternal(request, evaluateAutoHeaders: true, reloadAutoheaders: false);
        }
        private async Task<HttpResponseMessage> ExecuteInternal(StoredRequest request, bool evaluateAutoHeaders = true, bool reloadAutoheaders = false)
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
                        var header = await EvaluateAutoHeaderCached(autoHeader, request.Id, reloadAutoheaders);
                        if (header is not null)
                        {
                            httpRequest.Headers.Add(header.Key, header.Value);
                        }
                    }
                }
                if (evaluateAutoHeaders)
                {
                    SetStatus($"Executing: {request.Name}");
                }
                if (request.ContentType is ContentTypeJson)
                {
                    try
                    {
                        var obj = JsonSerializer.Deserialize<dynamic>(request.Body);
                        httpRequest.Content = JsonContent.Create(obj ?? "");
                    }
                    catch (JsonException ex)
                    {
                        SetStatus($"invalid json object as request body");
                    }
                }
                else if (request.ContentType is ContentTypeFormEncoded)
                {
                    httpRequest.Content = ContentConversion.StringToFormContent(request.Body);
                }
                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode && !reloadAutoheaders)
                {
                    return await ExecuteInternal(request, true, true);
                }
                var cookies = cookieContainer.GetAllCookies();
                if (cookies is not null && cookies.Count > 0)
                {
                    response.Headers.Add("Set-Cookie", cookies.Aggregate("", (a, b) => $"{a}{b.Name}={b.Value};expires={b.Expires};").TrimEnd(';'));
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

        private async Task <CachedAuthHeader?> EvaluateAutoHeaderCached(AutoHeader autoHeader, Guid requestId, bool forceReload)
        {
            var parser = _autoHeaders.FirstOrDefault(a => a.Guid == autoHeader.Type);
            SetStatus($"Loading: {parser?.Name ?? "Autoheader"}");
            if (_cachedHeaders.TryGetValue(requestId, out var cachedHeader))
            {
                if (cachedHeader.Expires > DateTime.Now && !forceReload)
                {
                    return cachedHeader;
                }
                else
                {
                    _cachedHeaders.Remove(requestId);
                }
            }
            var request = await _storage.GetById(autoHeader.RequestId);
            if (request == null)
            {
                return null;
            }
            var response = await ExecuteInternal(request, false);
            var header = await ParseHeaderValue(autoHeader.Type, response);
            _cachedHeaders.Add(requestId, header);
            return header;
        }

        private async Task<CachedAuthHeader> ParseHeaderValue(Guid type, HttpResponseMessage message)
        {
            var parser = _autoHeaders.FirstOrDefault(a => a?.Guid == type);
            if (parser is null)
            {
                throw new NotImplementedException($"No parser could be found for header type {type}");
            }
            var parsedResult = await parser.ParseHeader(message);
            return new(parsedResult.Key, parsedResult.Value, parsedResult.Expiration);
        }

        private void SetStatus(string status)
        {
            _intercom.Notify("RequestExecutionStatus", status);
        }

        public List<IAutoHeaderParser> AutoHeaders { get => _autoHeaders; set { } }
    }
}
