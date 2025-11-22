using CarMaintenanceDiary.Infrastructure.Identity;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.UserName,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.LockoutEnabled,
                    user.LockoutEnd,
                    roles,
                    user.ProfilePicture != null ? $"/api/usermanagement/{user.Id}/profile-picture" : null
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
            return Ok(new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.UserName,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                roles,
                user.ProfilePicture != null ? $"/api/usermanagement/{user.Id}/profile-picture" : null
            ));
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            return CreatedAtAction(nameof(GetUser), new
            {
                id = user.Id
            }, new UserDto(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                roles,
                null
            ));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (id != request.Id)
                return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

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

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

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
    }
}
