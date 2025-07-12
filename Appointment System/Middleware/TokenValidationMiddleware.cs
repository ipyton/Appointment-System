using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Appointment_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Appointment_System.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        // List of paths that should be publicly accessible without authentication
        private readonly string[] _publicPaths = new[] 
        { 
            "/search/suggest",  // Allow public access to search/suggest endpoint
            "/swagger",         // Allow access to Swagger UI
            "/graphql"          // Allow public access to GraphQL endpoint
        };

        public TokenValidationMiddleware(
            RequestDelegate next,
            ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the current path is in the public paths list
            string path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Skip token validation for public paths
            if (path != null && _publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            
            try
            {
                _logger.LogInformation("TokenValidationMiddleware");
                // Only check if we have authorization header
                // if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
                //     authHeader.ToString().StartsWith("Bearer "))
                // {
                //     string token = authHeader.ToString().Substring("Bearer ".Length).Trim();
                    
                //     // Check if token is valid
                //     var tokenService = context.RequestServices.GetRequiredService<TokenService>();
                //     var validationResult = await tokenService.ValidateToken(token);

                //     if (!validationResult.Succeeded)
                //     {
                //         _logger.LogWarning("Request with invalid token rejected");
                //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //         await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired token" });
                //         return;
                //     }
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token in middleware");
                // Continue to next middleware even if there's an error checking the blacklist
                // The JWT validation will still occur later in the pipeline
            }

            // Continue to the next middleware
            await _next(context);
        }
    }
} 