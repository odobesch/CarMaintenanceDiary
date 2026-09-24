using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Infrastructure.Identity;
using CarMaintenanceDiary.Model.Entities;
using CarMaintenanceDiary.Model.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        //[HttpPost("register")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        //{
        //    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //    var result = await _userManager.CreateAsync(user, model.Password);
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(result.Errors);
        //    }

        //    // assign default role
        //    await _userManager.AddToRoleAsync(user, "User");

        //    return Ok();
        //}

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseModel>> LoginAsync([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null)
                return Unauthorized(new
                {
                    message = "Invalid credentials"
                });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Unauthorized(new
                {
                    message = "Invalid credentials"
                });

            if (!user.EmailConfirmed)
                return Unauthorized(new
                {
                    message = "Email not confirmed"
                });

            var token = await GenerateJwtTokenAsync(user, isRefreshToken: false);
            var refreshToken = await GenerateJwtTokenAsync(user, isRefreshToken: true);
                        
            await _userManager.SetAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken", refreshToken);

            return Ok(new LoginResponseModel
            {
                Token = token,
                RefreshToken = refreshToken,
                TokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
            });
        }       

        [HttpGet("loginByRefeshToken")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseModel>> LoginByRefeshToken(string refreshToken)
        {
            var principal = ValidateToken(refreshToken, isRefreshToken: true);
            if (principal == null)
                return Unauthorized(new
                {
                    message = "Invalid refresh token"
                });

            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new
                {
                    message = "Invalid token claims"
                });

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return Unauthorized(new
                {
                    message = "User not found"
                });

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken");
            if (storedToken != refreshToken)
                return Unauthorized(new
                {
                    message = "Invalid refresh token"
                });

            var newToken = await GenerateJwtTokenAsync(user, isRefreshToken: false);
            var newRefreshToken = await GenerateJwtTokenAsync(user, isRefreshToken: true);

            await _userManager.SetAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken", newRefreshToken);

            return Ok(new LoginResponseModel
            {
                Token = newToken,
                TokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
                RefreshToken = newRefreshToken,
            });
        }       

        [HttpPost("logout")]        
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.RemoveAuthenticationTokenAsync(user, "CarMaintenanceDiary", "RefreshToken");
            if (!result.Succeeded)
                _logger.LogWarning("Failed to remove refresh token for user {Username}: {Errors}",
                    username, string.Join(", ", result.Errors.Select(e => e.Description)));

            await _signInManager.SignOutAsync();
            return NoContent();
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, bool isRefreshToken)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtSection = _configuration.GetSection("Jwt");
            var secret = isRefreshToken
                ? jwtSection["RefreshTokenSecret"]
                : jwtSection["Secret"];

            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException($"JWT {(isRefreshToken ? "RefreshTokenSecret" : "Secret")} not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(isRefreshToken ? 24 * 60 : 30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal? ValidateToken(string token, bool isRefreshToken)
        {
            try
            {
                var jwtSection = _configuration.GetSection("Jwt");
                var secret = isRefreshToken
                    ? jwtSection["RefreshTokenSecret"]
                    : jwtSection["Secret"];

                if (string.IsNullOrEmpty(secret))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }        
}
