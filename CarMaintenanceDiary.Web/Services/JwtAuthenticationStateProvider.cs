using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CarMaintenanceDiary.Web.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenStore _tokenStore;
        private readonly JwtSecurityTokenHandler _handler = new();
        private readonly ProtectedSessionStorage _sessionStorage;

        public JwtAuthenticationStateProvider(ITokenStore tokenStore, ProtectedSessionStorage sessionStorage)
        {
            _tokenStore = tokenStore;
            _sessionStorage = sessionStorage;
            _tokenStore.TokenChanged += OnTokenChanged;
        }

        private void OnTokenChanged(string? token)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // First try the in-memory store
            var token = _tokenStore.GetToken();

            // If not present in memory, try to load from browser session storage
            if (string.IsNullOrEmpty(token))
            {
                try
                {
                    var result = await _sessionStorage.GetAsync<string>("accessToken");
                    if (result.Success && !string.IsNullOrEmpty(result.Value))
                    {
                        token = result.Value;
                        // update in-memory cache so subsequent sync callers see it
                        await _tokenStore.SetTokenAsync(token);
                    }
                }
                catch
                {
                    // ignore storage issues; fall back to anonymous
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }

            try
            {
                var jwt = _handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            // token might be returned along with refresh token; leave refresh handling to token store callers
            await _tokenStore.SetTokenAsync(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _tokenStore.SetTokensAsync(null, null);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}