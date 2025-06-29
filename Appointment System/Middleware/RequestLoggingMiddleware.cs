using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            
            // Log the request
            _logger.LogInformation("Request {RequestId} started: {Method} {Path}{QueryString}",
                requestId, context.Request.Method, context.Request.Path, context.Request.QueryString);
            
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
                
                // Log the response
                sw.Stop();
                _logger.LogInformation("Request {RequestId} completed with status code {StatusCode} in {ElapsedMs}ms",
                    requestId, context.Response.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                sw.Stop();
                _logger.LogError(ex, "Request {RequestId} failed with exception in {ElapsedMs}ms",
                    requestId, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
    
    // Extension method to add the middleware to the request pipeline
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
} 