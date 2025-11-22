using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CarMaintenanceDiary.Web.Services
{
    // In-memory cache + persistence to browser session storage (ProtectedSessionStorage).
    // This keeps the fast in-memory access for server-side requests while persisting tokens
    // across F5/reloads by using the browser's sessionStorage (protected).
    public class InMemoryTokenStore : ITokenStore
    {
        private string? _token;
        private string? _refresh;
        private readonly ProtectedSessionStorage _sessionStorage;

        public event Action<string?>? TokenChanged;

        public InMemoryTokenStore(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public string? GetToken() => _token;
        public string? GetRefreshToken() => _refresh;

        public async Task SetTokensAsync(string? token, string? refreshToken)
        {
            _token = token;
            _refresh = refreshToken;

            try
            {
                await _sessionStorage.SetAsync("accessToken", token);
                await _sessionStorage.SetAsync("refreshToken", refreshToken);
            }
            catch
            {
                // ignore JS/protection errors - persistence is best-effort
            }

            TokenChanged?.Invoke(_token);
            // Remove: return Task.CompletedTask;
        }

        public async Task SetTokenAsync(string? accessToken)
        {
            _token = accessToken;

            try
            {
                await _sessionStorage.SetAsync("accessToken", accessToken);
            }
            catch
            {
                // ignore
            }

            TokenChanged?.Invoke(_token);
            // Remove: return Task.CompletedTask;
        }
    }
}