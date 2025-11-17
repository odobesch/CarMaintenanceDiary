using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using CarMaintenanceDiary.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace CarMaintenanceDiary.Web.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = "/")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                try
                {
                    var stored = await _userManager.GetAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken");
                    _logger.LogInformation("Logout page called for user {UserId}. Stored refresh present: {HasRefresh}", user.Id, !string.IsNullOrEmpty(stored));

                    if (!string.IsNullOrEmpty(stored))
                    {
                        await _userManager.RemoveAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove stored refresh token for user {UserId}", user.Id);
                }
            }
            else
            {
                _logger.LogInformation("Logout page called but no user principal found.");
            }

            // Sign out cookie-based schemes if present
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

            // Redirect to returnUrl (only local urls)
            if (Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return Redirect("/");
        }
    }
}