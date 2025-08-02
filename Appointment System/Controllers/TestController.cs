using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment_System.Services;
using Appointment_System.Models;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Appointment_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IServiceBusService _serviceBusService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IServiceBusService serviceBusService,
            IConfiguration configuration,
            ILogger<TestController> logger)
        {
            _serviceBusService = serviceBusService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to verify Service Bus connection
        /// </summary>
        /// <returns>Success message if test passes</returns>
        [HttpGet("servicebus")]
        public async Task<IActionResult> TestServiceBus()
        {
            try
            {
                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Create a test message
                var testMessage = new Message
                {
                    SenderId = currentUserId ?? "test-user",
                    ReceiverId = "test-receiver",
                    Content = "This is a test message from the Service Bus test endpoint",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    GroupName = "test-group"
                };
                
                // Send via service bus
                await _serviceBusService.SendMessageAsync(testMessage);
                
                // Get connection details for the response (excluding sensitive info)
                var connectionString = _configuration["ServiceBus:ConnectionString"] ?? "Not configured";
                var queueName = _configuration["ServiceBus:QueueName"] ?? "Not configured";
                
                // Return sanitized connection info (hiding the actual key)
                var endpoint = connectionString.Contains("Endpoint=") ? 
                    connectionString.Split(';')[0] : "No endpoint found";
                
                return Ok(new
                {
                    Status = "Success",
                    Message = "Test message sent to Service Bus",
                    QueueEndpoint = endpoint,
                    QueueName = queueName,
                    MessageId = testMessage.Id,
                    SentAt = testMessage.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Service Bus connection");
                return StatusCode(500, $"Service Bus test failed: {ex.Message}");
            }
        }
    }
} 