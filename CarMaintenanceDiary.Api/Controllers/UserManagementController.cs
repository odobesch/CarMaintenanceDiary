using CarMaintenanceDiary.Infrastructure.Identity;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger,
            IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                string? picUrl = user.ProfilePicture != null
                   ? $"{Request.Scheme}://{Request.Host}/api/usermanagement/{user.Id}/profile-picture"
                   : null;

                userDtos.Add(new UserDto(
                    user.Id,
                    user.FirstName ?? string.Empty,
                    user.LastName ?? string.Empty,
                    user.Email ?? string.Empty,
                    user.UserName,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.LockoutEnabled,
                    user.LockoutEnd,
                    roles,
                    picUrl
                ));
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            string? picUrl = user.ProfilePicture != null
                ? $"{Request.Scheme}://{Request.Host}/api/usermanagement/{user.Id}/profile-picture"
                : null;

            return Ok(new UserDto(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                user.UserName,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                roles,
                picUrl
            ));
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,                
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false
            };

            IdentityResult result;
            
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result = await _userManager.CreateAsync(user);
            }
            else
            {
                result = await _userManager.CreateAsync(user, request.Password);
            }

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
            }           

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                roles,
                null
            );           

            return CreatedAtAction(nameof(GetUser), new
            {
                id = user.Id
            }, userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (id != request.Id)
                return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UserName = request.UserName ?? request.Email;
            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        /// <summary>
        /// Activate user (set EmailConfirmed = true)
        /// POST /api/usermanagement/{id}/activate
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.EmailConfirmed)
                return NoContent(); // idempotent

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        /// <summary>
        /// Deactivate user (set EmailConfirmed = false)
        /// POST /api/usermanagement/{id}/deactivate
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (!user.EmailConfirmed)
                return NoContent(); // idempotent

            user.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPost("{id}/change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            if (id != request.UserId)
                return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            if (id != request.UserId)
                return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Admin-initiated reset: generate token server-side and apply it
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        /// <summary>
        /// Allow end user to set password using a token previously issued (reset token must be Base64Url encoded).
        /// POST /api/usermanagement/{id}/set-password
        /// Body: { userId, token, newPassword }
        /// </summary>
        [HttpPost("{id}/set-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword(string id, [FromBody] SetPasswordRequest request)
        {
            if (id != request.UserId)
                return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Missing token or password");

            string decodedToken;
            try
            {
                var bytes = WebEncoders.Base64UrlDecode(request.Token);
                decodedToken = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return BadRequest("Invalid token encoding");
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPost("{id}/profile-picture")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadProfilePicture(string id, [FromForm] IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            user.ProfilePicture = ms.ToArray();
            user.ProfilePictureContentType = file.ContentType;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                Url = $"/api/usermanagement/{user.Id}/profile-picture"
            });
        }

        [HttpGet("{id}/profile-picture")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfilePicture(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user?.ProfilePicture == null)
                return NotFound();

            return File(user.ProfilePicture, user.ProfilePictureContentType ?? "image/jpeg");
        }

        [HttpDelete("{id}/profile-picture")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteProfilePicture(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.ProfilePicture = null;
            user.ProfilePictureContentType = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpGet("roles")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Confirm email endpoint. Token must be Base64Url encoded.
        /// Example: GET /api/usermanagement/confirm-email?userId=...&token=...&resetToken=...
        /// </summary>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Missing userId or token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            string decodedToken;
            try
            {
                var bytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return BadRequest("Invalid token encoding");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                Message = "Email confirmed"
            });
        }

        /// <summary>
        /// Generate (and send, if IEmailSender is configured) confirmation link for the user.
        /// Returns the link in response if no IEmailSender is configured.
        /// Now includes a password reset token so the user can set password after confirming.
        /// </summary>
        [HttpPost("{id}/send-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> SendConfirmation(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "UserManagement",
                new
                {
                    userId = user.Id,
                    token = encodedToken,
                    resetToken = encodedResetToken
                },
                Request.Scheme) ?? string.Empty;

            var emailSender = _serviceProvider.GetService(typeof(IEmailSender)) as IEmailSender;
            if (emailSender != null && !string.IsNullOrEmpty(user.Email))
            {
                await emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by clicking this link: <a href=\"{callbackUrl}\">Confirm email</a>");
                return Ok(new
                {
                    Message = "Confirmation email sent"
                });
            }

            _logger.LogInformation("Email sender not configured - confirmation link: {Link}", callbackUrl);
            return Ok(new
            {
                ConfirmationLink = callbackUrl
            });
        }
    }

    public record SetPasswordRequest(string UserId, string Token, string NewPassword);
}
