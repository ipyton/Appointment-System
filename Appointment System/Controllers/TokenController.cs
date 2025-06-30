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
        public async Task<IActionResult> ValidateToken([FromBody] TokenDTO model)
        {
            _logger.LogInformation("Validation attempt for token");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during token validation attempt");
                return BadRequest(ModelState);
            }

            // Verify credentials
            var result = await _tokenService.ValidateToken(model.Token);
            
            if (result.Succeeded)
            {
                // Decode the JWT token to extract user information
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(model.Token);
                
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
                
                _logger.LogInformation("Token validated successfully for user: {UserId}", user.Id);
                
                return Ok(new {
                        email = user.Email,
                        fullName = user.FullName,
                        role = roles.FirstOrDefault(),
                        isServiceProvider = user.IsServiceProvider,
                        profilePictureUrl = user.ProfilePictureUrl,
                        businessName = user.BusinessName,
                        businessDescription = user.BusinessDescription
                    
                });
            }
            
            _logger.LogWarning("Invalid login attempt during token validation");
            return Unauthorized(new { message = "Invalid credentials" });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] TokenDTO model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during token revocation attempt");
                return BadRequest(ModelState);
            }
            
            await _tokenService.BlacklistToken(model.Token);
            _logger.LogInformation("Token revoked for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            
            return Ok(new { message = "Token revoked successfully" });
        }

        [Authorize]
        [HttpPost("revoke-current")]
        public async Task<IActionResult> RevokeCurrentToken()
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