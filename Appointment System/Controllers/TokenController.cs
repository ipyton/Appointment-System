using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using Appointment_System.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Linq;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TokenController> _logger;

        public TokenController(
            TokenService tokenService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenRecord model)
        {
            _logger.LogInformation("Validation attempt for token: {Token}", model.AccessToken);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during token validation attempt");
                return BadRequest(ModelState);
            }

            // Verify credentials
            var result = await _tokenService.ValidateToken(model.AccessToken);
            
            if (result.Succeeded)
            {
                // Decode the JWT token to extract user information
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(model.AccessToken);
                
                // Extract user ID from the token
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "Invalid token format" });
                }
                
                // Get user from database
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                
                var roles = await _userManager.GetRolesAsync(user);
                
                // Generate JWT token
                // var token = await _tokenService.GenerateJwtToken(user);
                
                _logger.LogInformation("Token generated successfully for user: {UserId}", user.Id);
                
                return Ok(new { 
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName,
                        roles = roles
                    }
                });
            }
            
            _logger.LogWarning("Invalid login attempt during token validation");
            return Unauthorized(new { message = "Invalid credentials" });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Token revocation attempted without valid authorization header");
                return BadRequest(new { message = "No token provided" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            await _tokenService.BlacklistToken(token);
            _logger.LogInformation("Token revoked for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            
            return Ok(new { message = "Token revoked successfully" });
        }
    }
} 