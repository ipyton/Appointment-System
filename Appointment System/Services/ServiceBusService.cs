using Azure.Messaging.ServiceBus;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Appointment_System.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Services
{
    public interface IServiceBusService
    {
        Task SendMessageAsync(Message message);
    }

    public class ServiceBusService : IServiceBusService
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
        {
            _connectionString = configuration["ServiceBus:ConnectionString"] ?? 
                throw new InvalidOperationException("Service Bus connection string is not configured");
            _queueName = configuration["ServiceBus:QueueName"] ?? "messages";
            _logger = logger;
        }

        public async Task SendMessageAsync(Message message)
        {
            try
            {
                // Create a ServiceBusClient
                await using var client = new ServiceBusClient(_connectionString);
                
                // Create a sender
                ServiceBusSender sender = client.CreateSender(_queueName);
                
                // Serialize the message to JSON
                string messageJson = JsonSerializer.Serialize(message);
                
                // Create a ServiceBusMessage
                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageJson)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Subject = "NewMessage",
                    ApplicationProperties = 
                    {
                        { "SenderId", message.SenderId },
                        { "ReceiverId", message.ReceiverId },
                        { "CreatedAt", message.CreatedAt.ToString("o") }
                    }
                };
                
                // Send the message
                await sender.SendMessageAsync(serviceBusMessage);
                
                _logger.LogInformation("Message sent to Service Bus: {MessageId} from {SenderId} to {ReceiverId}", 
                    serviceBusMessage.MessageId, message.SenderId, message.ReceiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to Service Bus");
                throw;
            }
        }
    }
} 