using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Appointment_System.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SecureController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SecureController> _logger;

        public SecureController(
            UserManager<ApplicationUser> userManager,
            ILogger<SecureController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Profile request for user: {UserId}", userId);
            
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("Profile request failed - user not found: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Profile retrieved for user: {UserId}, Roles: {Roles}", 
                userId, string.Join(", ", roles));
            
            return Ok(new { 
                user.Email, 
                user.FullName,
                user.UserName,
                Roles = roles
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Admin-only endpoint accessed by user: {UserId}", userId);
            
            return Ok(new { message = "You are an admin!" });
        }
    }
} 