using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Appointment_System.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

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
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService,
            ILogger<AccountController> logger,
            SearchIndexingEventHandler searchIndexingHandler,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
            _searchIndexingHandler = searchIndexingHandler;
            _configuration = configuration;
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
                    else
                    {
                        // Add the requested role to the existing user
                        _logger.LogInformation("Adding role {Role} to existing user {UserId}", model.Role, existingUser.Id);
                        var roleResult = await _userManager.AddToRoleAsync(existingUser, model.Role);
                        
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Role {Role} added to existing user {UserId}", model.Role, existingUser.Id);
                            
                            // Update IsServiceProvider flag if needed
                            if (model.Role == "ServiceProvider" && !existingUser.IsServiceProvider)
                            {
                                existingUser.IsServiceProvider = true;
                                await _userManager.UpdateAsync(existingUser);
                                _logger.LogInformation("Updated IsServiceProvider flag for user {UserId}", existingUser.Id);
                            }
                            
                            return Ok(new { message = "Role added successfully", statusCode = 200 });
                        }
                        
                        _logger.LogWarning("Failed to add role {Role} to user {UserId}", model.Role, existingUser.Id);
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
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
        [HttpGet("login")]
        public IActionResult GetLogin()
        {
            return Ok(new { message = "Please use POST method for login" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}, role: {Role}", model.Email, model.Role);
                
                if (ModelState.IsValid)
                {
                    // Validate role
                    if (model.Role != "ServiceProvider" && model.Role != "User")
                    {
                        _logger.LogWarning("Invalid role specified: {Role}", model.Role);
                        return StatusCode(400, new { 
                            statusCode = 400,
                            message = "Role must be either 'ServiceProvider' or 'User'"
                        });
                    }
                    
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email, 
                        model.Password, 
                        model.RememberMe, 
                        lockoutOnFailure: true);
                    
                    if (result.Succeeded)
                        {
                            var user = await _userManager.FindByEmailAsync(model.Email);
                            var roles = await _userManager.GetRolesAsync(user);
                            
                            // Check if the user has the requested role
                            if (!roles.Contains(model.Role))
                            {
                                _logger.LogInformation("Adding role {Role} to user {UserId}", model.Role, user.Id);
                                var addToRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                                
                                if (!addToRoleResult.Succeeded)
                                {
                                    _logger.LogWarning("Failed to add role {Role} to user {UserId}", model.Role, user.Id);
                                    return StatusCode(500, new { 
                                        statusCode = 500,
                                        message = "Failed to add role to user",
                                        errors = addToRoleResult.Errors
                                    });
                                }
                                
                                // Update IsServiceProvider flag if needed
                                if (model.Role == "ServiceProvider" && !user.IsServiceProvider)
                                {
                                    user.IsServiceProvider = true;
                                    await _userManager.UpdateAsync(user);
                                    _logger.LogInformation("Updated IsServiceProvider flag for user {UserId}", user.Id);
                                }
                            }
                            
                            _logger.LogInformation("User logged in successfully: {UserId} with role {Role}", user?.Id, model.Role);
                            var token = await _tokenService.GenerateJwtToken(user, model.RememberMe);
                        
                            await _tokenService.SetToken(token);
                            return StatusCode(200, new { 
                                statusCode = 200,
                                message = "User logged in successfully",
                                userId = user.Id,
                                email = user.Email,
                                fullName = user.FullName,
                                isServiceProvider = user.IsServiceProvider,
                                role = model.Role,
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
                return StatusCode(500, new { 
                    statusCode = 500, 
                    message = "Internal server error during login", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
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

        // External Authentication
        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> GoogleSignIn([FromBody] ExternalAuthDto authData)
        {
            try
            {
                _logger.LogInformation("Google sign-in attempt");
                
                if (string.IsNullOrEmpty(authData.Token))
                {
                    return BadRequest(new { message = "Token is required" });
                }
                
                // Validate role
                if (authData.Role != "ServiceProvider" && authData.Role != "User")
                {
                    _logger.LogWarning("Invalid role specified: {Role}", authData.Role);
                    return StatusCode(400, new { 
                        statusCode = 400,
                        message = "Role must be either 'ServiceProvider' or 'User'"
                    });
                }
                
                // Validate the token with Google
                var userInfo = await ValidateGoogleToken(authData.Token);
                if (userInfo == null)
                {
                    _logger.LogWarning("Invalid Google token");
                    return BadRequest(new { message = "Invalid Google authentication token" });
                }
                
                // Process the user info and return JWT token
                return await ProcessExternalUserInfo(userInfo, "Google", userInfo.ProviderKey, authData.RememberMe, authData.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google sign-in: {Message}", ex.Message);
                return StatusCode(500, new { 
                    statusCode = 500, 
                    message = "Internal server error during Google sign-in", 
                    error = ex.Message
                });
            }
        }
        
        [AllowAnonymous]
        [HttpPost("github")]
        public async Task<IActionResult> GitHubSignIn([FromBody] ExternalAuthDto authData)
        {
            try
            {
                _logger.LogInformation("GitHub sign-in attempt");
                
                if (string.IsNullOrEmpty(authData.Token))
                {
                    return BadRequest(new { message = "Token is required" });
                }
                
                // Validate role
                if (authData.Role != "ServiceProvider" && authData.Role != "User")
                {
                    _logger.LogWarning("Invalid role specified: {Role}", authData.Role);
                    return StatusCode(400, new { 
                        statusCode = 400,
                        message = "Role must be either 'ServiceProvider' or 'User'"
                    });
                }
                
                // Validate the token with GitHub
                var userInfo = await ValidateGitHubToken(authData.Token);
                if (userInfo == null)
                {
                    _logger.LogWarning("Invalid GitHub token");
                    return BadRequest(new { message = "Invalid GitHub authentication token" });
                }
                
                // Process the user info and return JWT token
                return await ProcessExternalUserInfo(userInfo, "GitHub", userInfo.ProviderKey, authData.RememberMe, authData.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GitHub sign-in: {Message}", ex.Message);
                return StatusCode(500, new { 
                    statusCode = 500, 
                    message = "Internal server error during GitHub sign-in", 
                    error = ex.Message
                });
            }
        }
        
        // GitHub requires a server-side exchange of code for token
        [AllowAnonymous]
        [HttpPost("github-token-exchange")]
        public async Task<IActionResult> GitHubTokenExchange([FromBody] GitHubCodeExchangeRequest request)
        {
            try
            {
                _logger.LogInformation("GitHub code exchange request");
                
                if (string.IsNullOrEmpty(request.Code))
                {
                    return BadRequest(new { message = "Code is required" });
                }
                
                // Get GitHub OAuth settings
                var clientId = _configuration["Authentication:GitHub:ClientId"];
                var clientSecret = _configuration["Authentication:GitHub:ClientSecret"];
                
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("GitHub client credentials not configured");
                    return StatusCode(500, new { message = "Server configuration error" });
                }
                
                // Exchange code for token
                using var client = new HttpClient();
                var parameters = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", request.Code },
                    { "redirect_uri", request.RedirectUri }
                };
                
                var content = new FormUrlEncodedContent(parameters);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to exchange GitHub code: {StatusCode}", response.StatusCode);
                    return BadRequest(new { message = "Failed to exchange code for token" });
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<GitHubTokenResponse>(responseContent);
                
                return Ok(new { access_token = tokenResponse.AccessToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GitHub code exchange: {Message}", ex.Message);
                return StatusCode(500, new { message = "Error exchanging GitHub code for token" });
            }
        }
        
        private async Task<ExternalUserInfo> ValidateGoogleToken(string token)
        {
            try
            {
                // Google client ID to validate the token against
                string clientId = "890925845237-v6896jvsm3pc4heeq21e22bsptcl4egg.apps.googleusercontent.com";
                
                // Use HttpClient to validate the token with Google's API
                using var client = new HttpClient();
                var encodedToken = Uri.EscapeDataString(token);
                var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={encodedToken}");
        
                if (!response.IsSuccessStatusCode)
                {
                        var errorContent = await response.Content.ReadAsStringAsync();

                    _logger.LogWarning($"Invalid Google token. Status: {response.StatusCode}, Response: {errorContent}");

                    //_logger.LogWarning("Invalid Google token");
                    return null;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenInfo>(content);
                
                // Verify the token was intended for our application
                if (tokenInfo.Aud != clientId)
                {
                    _logger.LogWarning($"Token was not issued for this application. Token: {tokenInfo.Aud}, Client ID: {clientId}");
                    _logger.LogWarning("Token was not issued for this application");
                    return null;
                }
                
                return new ExternalUserInfo
                {
                    Email = tokenInfo.Email,
                    Name = tokenInfo.Name,
                    ProviderKey = tokenInfo.Sub,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }
        
        private async Task<ExternalUserInfo> ValidateGitHubToken(string token)
        {
            try
            {
                // Use HttpClient to validate the token with GitHub's API
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"token {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "AppointmentSystem");
                
                // Get user info
                var userResponse = await client.GetAsync("https://api.github.com/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Invalid GitHub token");
                    return null;
                }
                
                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userInfo = System.Text.Json.JsonSerializer.Deserialize<GitHubUserInfo>(userContent);
                
                // GitHub doesn't always return email, so we might need an additional request
                string email = userInfo.Email;
                if (string.IsNullOrEmpty(email))
                {
                    var emailsResponse = await client.GetAsync("https://api.github.com/user/emails");
                    if (emailsResponse.IsSuccessStatusCode)
                    {
                        var emailsContent = await emailsResponse.Content.ReadAsStringAsync();
                        var emails = System.Text.Json.JsonSerializer.Deserialize<List<GitHubEmailInfo>>(emailsContent);
                        var primaryEmail = emails.FirstOrDefault(e => e.Primary);
                        if (primaryEmail != null)
                        {
                            email = primaryEmail.Email;
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("GitHub token valid but no email provided");
                    return null;
                }
                
                return new ExternalUserInfo
                {
                    Email = email,
                    Name = userInfo.Name ?? userInfo.Login,
                    ProviderKey = userInfo.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating GitHub token");
                return null;
            }
        }

        private async Task<IActionResult> ProcessExternalUserInfo(ExternalUserInfo userInfo, string provider, string providerKey, bool rememberMe, string role = "User")
        {
            // Validate role
            if (role != "ServiceProvider" && role != "User")
            {
                _logger.LogWarning("Invalid role specified: {Role}", role);
                return StatusCode(400, new { 
                    statusCode = 400,
                    message = "Role must be either 'ServiceProvider' or 'User'"
                });
            }
            
            // Check if user exists with this external login
            var existingLogin = await _userManager.FindByLoginAsync(provider, providerKey);
            if (existingLogin != null)
            {
                _logger.LogInformation("User logged in with external provider: {Provider}", provider);
                
                // Check if the user has the requested role
                var roles = await _userManager.GetRolesAsync(existingLogin);
                _logger.LogInformation("Roles: {Roles}", string.Join(", ", roles));
                if (!roles.Contains(role))
                {
                    _logger.LogInformation("Adding role {Role} to user {UserId}", role, existingLogin.Id);
                    var addToRoleResult = await _userManager.AddToRoleAsync(existingLogin, role);
                    
                    if (!addToRoleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to add role {Role} to user {UserId}", role, existingLogin.Id);
                        return StatusCode(500, new { 
                            statusCode = 500,
                            message = "Failed to add role to user",
                            errors = addToRoleResult.Errors
                        });
                    }
                    
                    // Update IsServiceProvider flag if needed
                    if (role == "ServiceProvider" && !existingLogin.IsServiceProvider)
                    {
                        existingLogin.IsServiceProvider = true;
                        await _userManager.UpdateAsync(existingLogin);
                        _logger.LogInformation("Updated IsServiceProvider flag for user {UserId}", existingLogin.Id);
                    }
                }
                
                // Generate JWT token for existing user
                var token = await _tokenService.GenerateJwtToken(existingLogin, rememberMe);
                await _tokenService.SetToken(token);
                
                return StatusCode(200, new {
                    statusCode = 200,
                    message = "User logged in successfully",
                    userId = existingLogin.Id,
                    email = existingLogin.Email,
                    fullName = existingLogin.FullName,
                    isServiceProvider = existingLogin.IsServiceProvider,
                    role = role,
                    profilePictureUrl = existingLogin.ProfilePictureUrl,
                    businessName = existingLogin.BusinessName,
                    businessDescription = existingLogin.BusinessDescription,
                    token = token.AccessToken
                });
            }
            
            // Check if user with same email exists
            var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
            ApplicationUser user;
            
            if (existingUser == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    FullName = userInfo.Name ?? userInfo.Email.Split('@')[0],
                    EmailConfirmed = true,
                    Address = "Not specified",
                    BusinessName = null,
                    BusinessDescription = null,
                    ProfilePictureUrl = "",
                    IsServiceProvider = false
                };
                
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to create user from external login: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(result.Errors);
                }
                
                // Add to specified role
                await _userManager.AddToRoleAsync(user, role);
                
                // Index the new user in Azure Search
                await _searchIndexingHandler.UserCreatedOrUpdatedAsync(user);
            }
            else
            {
                user = existingUser;
            }
            
            // Add external login
            var info = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                _logger.LogWarning("Failed to add external login to user: {Errors}", 
                    string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                return BadRequest(addLoginResult.Errors);
            }
            
            // Generate JWT token
            var newToken = await _tokenService.GenerateJwtToken(user, rememberMe);
            await _tokenService.SetToken(newToken);
            var userRoles = await _userManager.GetRolesAsync(user);
            
            return StatusCode(200, new {
                statusCode = 200,
                message = "User logged in successfully with external provider",
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                isServiceProvider = user.IsServiceProvider,
                role = role,
                profilePictureUrl = user.ProfilePictureUrl,
                businessName = user.BusinessName,
                businessDescription = user.BusinessDescription,
                token = newToken.AccessToken
            });
        }
    }
    
    public class ExternalAuthDto
    {
        public string Token { get; set; }
        public bool RememberMe { get; set; }
        [Required]
        [RegularExpression("^(ServiceProvider|User)$", ErrorMessage = "Role must be either 'ServiceProvider' or 'User'")]
        public string Role { get; set; } = "User";
    }
    
    public class ExternalUserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string ProviderKey { get; set; }
    }
    

    public class GoogleTokenInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("email_verified")]
        [JsonConverter(typeof(StringToBooleanConverter))]
        public bool Email_verified { get; set; }
        
        [JsonPropertyName("aud")]
        public string Aud { get; set; }
    }

    public class StringToBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string value = reader.GetString();
                return bool.TryParse(value, out bool result) && result;
            }
            
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }


    public class GitHubUserInfo
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    
    public class GitHubEmailInfo
    {
        public string Email { get; set; }
        public bool Primary { get; set; }
        public bool Verified { get; set; }
    }

    // Request/response models for the controller
    public class GitHubCodeExchangeRequest
    {
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
    
    public class GitHubTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
} 