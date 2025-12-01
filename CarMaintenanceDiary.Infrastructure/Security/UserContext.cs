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
                _logger?.LogWarning("User is not authenticated. Identity: {Identity}", user.Identity?.Name ?? "null");
                return null;
            }

            // Try multiple claim types that might contain the user ID
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst("sub")?.Value
                      ?? user.FindFirst("userId")?.Value
                      ?? user.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger?.LogWarning("No user ID found in claims. Available claims: {Claims}", 
                    string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
            }
            else
            {
                _logger?.LogDebug("Found userId: {UserId}", userId);
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

            var isInRole = httpContext.User?.IsInRole(role) ?? false;
            _logger?.LogDebug("IsInRole({Role}): {Result}. User: {User}", 
                role, isInRole, httpContext.User?.Identity?.Name ?? "null");
            
            return isInRole;
        }
    }
}
