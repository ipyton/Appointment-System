using Azure.Messaging.ServiceBus;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Appointment_System.Models;
using Appointment_System.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Services
{
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger<ServiceBusConsumer> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMessageService _messageService;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public ServiceBusConsumer(
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger,
            IHubContext<ChatHub> hubContext,
            IMessageService messageService)
        {
            _connectionString = configuration["ServiceBus:ConnectionString"] ?? 
                throw new InvalidOperationException("Service Bus connection string is not configured");
            _queueName = configuration["ServiceBus:QueueName"] ?? "messages";
            _logger = logger;
            _hubContext = hubContext;
            _messageService = messageService;

            // Create the clients that we'll use for sending and processing messages.
            _client = new ServiceBusClient(_connectionString);
            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure the message handler
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing
            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("Service Bus Consumer started, listening to queue: {QueueName}", _queueName);

            try
            {
                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // This is expected when the stoppingToken is canceled
            }
            finally
            {
                // Stop processing
                await _processor.StopProcessingAsync();
                await _processor.DisposeAsync();
                await _client.DisposeAsync();
                
                _logger.LogInformation("Service Bus Consumer stopped");
            }
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                // Get the message body
                string messageBody = args.Message.Body.ToString();
                _logger.LogInformation("Received message: {MessageBody}", messageBody);

                // Deserialize the message
                var message = JsonSerializer.Deserialize<Message>(messageBody);

                if (message != null)
                {
                    // Get the group name for these two users
                    string groupName = message.GroupName ?? GetGroupName(message.SenderId, message.ReceiverId);
                    
                    // Send the message to SignalR clients
                    await _hubContext.Clients
                        .Group(groupName)
                        .SendAsync("ReceiveMessage", message);
                    
                    _logger.LogInformation("Message forwarded to SignalR: From {SenderId} to {ReceiverId}", 
                        message.SenderId, message.ReceiverId);
                }

                // Complete the message
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                
                // Abandon the message so it can be processed again later
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error handling Service Bus message: {ErrorSource}", args.ErrorSource);
            return Task.CompletedTask;
        }
        
        private string GetGroupName(string user1Id, string user2Id)
        {
            // Create a group name that will be the same regardless of which user ID comes first
            return user1Id.CompareTo(user2Id) < 0
                ? $"chat-{user1Id}-{user2Id}"
                : $"chat-{user2Id}-{user1Id}";
        }
    }
} 