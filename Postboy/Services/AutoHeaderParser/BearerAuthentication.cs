using System.Text.Json;
using Postboy.DTOs;

namespace Postboy.Services.AutoHeaderParser
{
    public class BearerAuthentication : IAutoHeaderParser
    {
        private const string _name = "BearerAuthentication";
        private static readonly Guid _guid = new("CBE36566-A686-45D9-A8FA-F2D010A1D278");
        public string Name { get => _name; set { } }

        public Guid Guid { get => _guid; set { } }

        public async Task<(string Key, string Value, DateTime Expiration)> ParseHeader(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponseDto>(content);
            return ("Authorization", $"Bearer {token?.AccessToken}", DateTime.Now + TimeSpan.FromSeconds(token?.ExpiresIn ?? 0));
        }
    }
}
