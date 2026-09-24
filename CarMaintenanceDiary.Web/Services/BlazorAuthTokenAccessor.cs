using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Model.Models;
using CarMaintenanceDiary.Web.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace CarMaintenanceDiary.Web.Services
{
    public class BlazorAuthTokenAccessor : IAuthTokenAccessor
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<BlazorAuthTokenAccessor> _logger;

        public BlazorAuthTokenAccessor(
            ProtectedLocalStorage localStorage,
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager,
            ILogger<BlazorAuthTokenAccessor> logger)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                _logger.LogDebug("Attempting to get token from ProtectedLocalStorage");
                var result = await _localStorage.GetAsync<LoginResponseModel>("sessionState");
                
                _logger.LogDebug("GetAsync result - Success: {Success}", result.Success);
                
                var sessionState = result.Value;

                if (sessionState == null)
                {
                    _logger.LogWarning("Session state is null - user may not be logged in");
                    return null;
                }
                
                _logger.LogDebug("SessionState retrieved - Token length: {TokenLength}, Token starts with: {TokenStart}, TokenExpired: {TokenExpired}, RefreshToken length: {RefreshTokenLength}",
                    sessionState.Token?.Length ?? 0,
                    sessionState.Token?.Length > 20 ? sessionState.Token[..20] : sessionState.Token,
                    sessionState.TokenExpired,
                    sessionState.RefreshToken?.Length ?? 0);

                if (string.IsNullOrEmpty(sessionState.Token))
                {
                    _logger.LogWarning("Token in session state is null or empty");
                    return null;
                }
                
                if (!sessionState.Token.StartsWith("eyJ"))
                {
                    _logger.LogError("Token does not look like a JWT! Starts with: {Start}", 
                        sessionState.Token.Length > 10 ? sessionState.Token[..10] : sessionState.Token);
                    return null;
                }

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                _logger.LogDebug("Time comparison - TokenExpired: {TokenExpired}, Now: {Now}, Difference: {Diff} seconds",
                    sessionState.TokenExpired, now, sessionState.TokenExpired - now);
               
                if (sessionState.TokenExpired > now + 60)
                {
                    _logger.LogDebug("Token is valid, returning token");
                    return sessionState.Token;
                }

                
                _logger.LogInformation("Token expired or expiring soon, attempting refresh");

                if (string.IsNullOrEmpty(sessionState.RefreshToken))
                {
                    _logger.LogWarning("No refresh token available");
                    await LogoutAndRedirect();
                    return null;
                }

                var newSession = await RefreshTokenAsync(sessionState.RefreshToken);
                if (newSession != null)
                {
                    _logger.LogInformation("Token refreshed successfully");
                    return newSession.Token;
                }

                _logger.LogWarning("Token refresh failed");
                await LogoutAndRedirect();
                return null;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("JavaScript not available (prerendering): {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting token");
                return null;
            }
        }

        private async Task<LoginResponseModel?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogDebug("Calling refresh token endpoint");
                var httpClient = _httpClientFactory.CreateClient("Api");
                var response = await httpClient.GetFromJsonAsync<LoginResponseModel>(
                    $"/api/auth/loginByRefeshToken?refreshToken={Uri.EscapeDataString(refreshToken)}");

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    await _localStorage.SetAsync("sessionState", response);

                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        await customProvider.MarkUserAsAuthenticatedAsync(response);
                    }

                    return response;
                }

                _logger.LogWarning("Refresh token response was null or empty");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
            }

            return null;
        }

        private async Task LogoutAndRedirect()
        {
            try
            {
                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    await customProvider.MarkUserAsLoggedOutAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            var returnUrl = Uri.EscapeDataString(_navigationManager.ToBaseRelativePath(_navigationManager.Uri));
            _navigationManager.NavigateTo($"/auth/login?returnUrl={returnUrl}", true);
        }
    }
}
