using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Appointment_System.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Appointment_System.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly ILogger<AccountController> _logger;
        private readonly SearchIndexingEventHandler _searchIndexingHandler;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService,
            ILogger<AccountController> logger,
            SearchIndexingEventHandler searchIndexingHandler)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
            _searchIndexingHandler = searchIndexingHandler;
        }
        
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", model.Email);
            
            if (ModelState.IsValid)
            {   
                // Check if user with the same email exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Check if user already has the requested role
                    var existingRoles = await _userManager.GetRolesAsync(existingUser);
                    if (existingRoles.Contains(model.Role))
                    {
                        _logger.LogWarning("Registration failed: User with email {Email} and role {Role} already exists", model.Email, model.Role);
                        ModelState.AddModelError(string.Empty, $"A user with email {model.Email} and role {model.Role} already exists");
                        return BadRequest(ModelState);
                    }
                }
                
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
                    
                    // Index the new user in Azure Search
                    await _searchIndexingHandler.UserCreatedOrUpdatedAsync(user);
                    
                    return Ok(new { message = "User registered successfully", statusCode = 200 });
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
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
                        var roles = await _userManager.GetRolesAsync(user);
                        _logger.LogInformation("User logged in successfully: {UserId}", user?.Id);
                        var token = await _tokenService.GenerateJwtToken(user, model.RememberMe);
                    
                        await _tokenService.SetToken(token);
                        return StatusCode(200, new { 
                            statusCode = 200,
                            message = "User logged in successfully",
                            userId = user.Id,
                            email = user.Email,
                            fullName = user.FullName,
                            isServiceProvider = user.IsServiceProvider,
                            role = roles.FirstOrDefault(),
                            profilePictureUrl = user.ProfilePictureUrl,
                            businessName = user.BusinessName,
                            businessDescription = user.BusinessDescription,
                            token = token.AccessToken
                        });
                    }
                    
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out: {Email}", model.Email);
                        return StatusCode(403, new { 
                            statusCode = 403,
                            message = "User account locked out"
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                        return StatusCode(401, new { 
                            statusCode = 401,
                            message = "Invalid login credentials" 
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid model state during login attempt");
                    return StatusCode(400, new { 
                        statusCode = 400,
                        message = "Invalid input data",
                        errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Message}", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = "Internal server error: " + ex.Message });
            }
        }
        
        [AllowAnonymous]
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