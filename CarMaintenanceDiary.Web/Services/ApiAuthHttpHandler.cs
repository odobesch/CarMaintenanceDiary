using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Web.Services
{
    public class ApiAuthHttpHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokenStore;
        private readonly JwtAuthenticationStateProvider _authProvider;
        private readonly NavigationManager _navigation;
        private readonly IHttpClientFactory _factory;
        private static readonly SemaphoreSlim _refreshLock = new(1,1);

        public ApiAuthHttpHandler(ITokenStore tokenStore, JwtAuthenticationStateProvider authProvider, NavigationManager navigation, IHttpClientFactory factory)
        {
            _tokenStore = tokenStore;
            _authProvider = authProvider;
            _navigation = navigation;
            _factory = factory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _tokenStore.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Attempt refresh once
                var refreshToken = _tokenStore.GetRefreshToken();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _refreshLock.WaitAsync(cancellationToken);
                    try
                    {
                        // maybe another concurrent request already refreshed
                        var current = _tokenStore.GetToken();
                        if (!string.IsNullOrEmpty(current))
                        {
                            // retry original request with latest token
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", current);
                            response.Dispose();
                            response = await base.SendAsync(request, cancellationToken);
                            if (response.StatusCode != HttpStatusCode.Unauthorized)
                                return response;
                        }

                        // Use a client without the auth handler to call refresh (avoid recursion)
                        var http = _factory.CreateClient("ApiNoAuth");
                        var refreshReq = new RefreshRequest { Token = token ?? string.Empty, RefreshToken = refreshToken };
                        HttpResponseMessage? refreshResp = null;
                        try
                        {
                            refreshResp = await http.PostAsJsonAsync("api/auth/refresh", refreshReq, cancellationToken);
                        }
                        catch
                        {
                            // ignore
                        }

                        if (refreshResp != null && refreshResp.IsSuccessStatusCode)
                        {
                            var content = await refreshResp.Content.ReadAsStringAsync(cancellationToken);
                            var doc = JsonDocument.Parse(content);
                            var newToken = doc.RootElement.GetProperty("token").GetString();
                            var newRefresh = doc.RootElement.GetProperty("refreshToken").GetString();
                            if (!string.IsNullOrEmpty(newToken))
                            {
                                await _tokenStore.SetTokensAsync(newToken, newRefresh);
                                await _authProvider.MarkUserAsAuthenticatedAsync(newToken);

                                // retry original request with new token
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                                response.Dispose();
                                response = await base.SendAsync(request, cancellationToken);
                                return response;
                            }
                        }
                    }
                    finally
                    {
                        _refreshLock.Release();
                    }
                }

                // fallback: sign out and redirect to login
                try
                {
                    await _authProvider.MarkUserAsLoggedOutAsync();
                }
                catch { }
                try
                {
                    var returnUrl = Uri.EscapeDataString(_navigation.ToBaseRelativePath(_navigation.Uri));
                    _navigation.NavigateTo($"/auth/login?returnUrl={returnUrl}", true);
                }
                catch { }
            }

            return response;
        }

        private class RefreshRequest { public string Token { get; set; } = string.Empty; public string RefreshToken { get; set; } = string.Empty; }
    }
}