using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Appointment_System.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", model.Email);
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = "Your Address",
                    BusinessName = "Your Business Name",
                    BusinessDescription = "Your Business Description",
                    ProfilePictureUrl = "",
                    IsServiceProvider = model.Role=="ServiceProvider"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    _logger.LogInformation("User created successfully: {UserId}", user.Id);
                    _logger.LogInformation("User signed in after registration: {UserId}", user.Id);
                    return Ok(new { message = "User registered successfully" });
                }
                
                _logger.LogWarning("Failed to create user for {Email}. Errors: {Errors}", 
                    model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state during registration attempt");
            }
            
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    _logger.LogInformation("User logged in successfully: {UserId}", user?.Id);
                    return Ok(new { message = "User logged in successfully" });
                }
                
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out: {Email}", model.Email);
                    return StatusCode(403, new { message = "User account locked out" });
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state during login attempt");
            }
            
            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Logout request for user: {UserId}", userId ?? "unknown");
            
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully: {UserId}", userId ?? "unknown");
            
            return Ok(new { message = "User logged out successfully" });
        }
    }
} 