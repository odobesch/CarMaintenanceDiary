using CarMaintenanceDiary.Model.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarMaintenanceDiary.Web.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage;

        public CustomAuthStateProvider(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sessionModel = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

                if (sessionModel == null)
                {
                    return CreateAnonymousState();
                }
                
                if (sessionModel.TokenExpired < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    // Token expired - let BlazorAuthTokenAccessor handle refresh
                    // Still return authenticated state so user can access pages
                    // The API call will trigger refresh
                }

                var identity = GetClaimsIdentity(sessionModel.Token);
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch (InvalidOperationException)
            {
                return CreateAnonymousState();
            }
            catch (Exception)
            {
                try
                {
                    await _localStorage.DeleteAsync("sessionState");
                }
                catch { }

                return CreateAnonymousState();
            }
        }

        public async Task MarkUserAsAuthenticatedAsync(LoginResponseModel model)
        {
            await _localStorage.SetAsync("sessionState", model);

            var identity = GetClaimsIdentity(model.Token);
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            try
            {
                await _localStorage.DeleteAsync("sessionState");
            }
            catch (InvalidOperationException) { }

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }

        private static AuthenticationState CreateAnonymousState()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
    }
}