using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CarMaintenanceDiary.Web.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenStore _tokenStore;
        private readonly JwtSecurityTokenHandler _handler = new();
        public JwtAuthenticationStateProvider(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
            _tokenStore.TokenChanged += OnTokenChanged;
        }

        private void OnTokenChanged(string? token)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = _tokenStore.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.FromResult(new AuthenticationState(anonymous));
            }

            try
            {
                var jwt = _handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
            catch
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.FromResult(new AuthenticationState(anonymous));
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