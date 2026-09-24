using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CarMaintenanceDiary.Infrastructure.Security
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserContext>? _logger;

        public UserContext(IHttpContextAccessor httpContextAccessor, ILogger<UserContext>? logger = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext == null)
            {
                _logger?.LogWarning("HttpContext is null in UserContext");
                return null;
            }

            var user = httpContext.User;
            if (user == null)
            {
                _logger?.LogWarning("HttpContext.User is null");
                return null;
            }

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger?.LogWarning("User is not authenticated. Identity: {Identity}, IsAuthenticated: {IsAuthenticated}", 
                    user.Identity?.Name ?? "null", 
                    user.Identity?.IsAuthenticated ?? false);
                return null;
            }
           
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst("sub")?.Value
                      ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                      ?? user.FindFirst("userId")?.Value
                      ?? user.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger?.LogWarning("No user ID found in claims. Available claims: {Claims}", 
                    string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
            }
            else
            {
                _logger?.LogInformation("Found userId: {UserId} from authenticated user", userId);
            }

            return userId;
        }

        public bool IsInRole(string role)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger?.LogWarning("HttpContext is null when checking role");
                return false;
            }

            var user = httpContext.User;
            if (user == null || !(user.Identity?.IsAuthenticated ?? false))
            {
                _logger?.LogWarning("User is null or not authenticated when checking role");
                return false;
            }
            
            var isInRole = user.IsInRole(role) 
                        || user.HasClaim(ClaimTypes.Role, role)
                        || user.HasClaim("role", role)
                        || user.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role);

            _logger?.LogInformation("IsInRole({Role}): {Result}. User: {User}, Claims: {Claims}", 
                role, isInRole, user.Identity?.Name ?? "null",
                string.Join(", ", user.Claims.Where(c => c.Type.Contains("role", StringComparison.OrdinalIgnoreCase)).Select(c => $"{c.Type}={c.Value}")));
            
            return isInRole;
        }
    }
}
