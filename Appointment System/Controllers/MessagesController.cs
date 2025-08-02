using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Hubs;
using Appointment_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Appointment_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IServiceBusService _serviceBusService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessageService messageService,
            IServiceBusService serviceBusService,
            ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _serviceBusService = serviceBusService;
            _logger = logger;
        }

        /// <summary>
        /// Gets conversation messages between current user and another user
        /// </summary>
        /// <param name="userId">The other user's ID</param>
        /// <returns>List of messages</returns>
        [HttpGet("conversation/{userId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetConversation(string userId)
        {
            try
            {
                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _messageService.GetConversationAsync(currentUserId, userId);
                return Ok(messages);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation with user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Gets unread messages for the current user
        /// </summary>
        /// <returns>List of unread messages</returns>
        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUnreadMessages()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var unreadMessages = await _messageService.GetUnreadMessagesAsync(currentUserId);
                return Ok(unreadMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread messages");
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Sends a new message to another user and broadcasts it via SignalR
        /// </summary>
        /// <param name="receiverId">The recipient user ID</param>
        /// <param name="messageContent">Message content</param>
        /// <returns>The created message</returns>
        [HttpPost("send")]
        public async Task<ActionResult<Message>> SendMessage(string receiverId, [FromBody] string messageContent)
        {
            try
            {
                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var newMessage = await _messageService.SendMessageAsync(currentUserId, receiverId, messageContent);
                
                // Also publish to Service Bus
                await _serviceBusService.SendMessageAsync(newMessage);
                
                return Ok(newMessage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {ReceiverId}", receiverId);
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        /// <summary>
        /// Sends a new message using a JSON payload that includes recipient and message content
        /// </summary>
        /// <param name="messageDto">Message data transfer object</param>
        /// <returns>The created message</returns>
        [HttpPost]
        public async Task<ActionResult<Message>> SendMessageJson([FromBody] MessageDto messageDto)
        {
            try
            {
                if (messageDto == null)
                {
                    return BadRequest("Message data is required");
                }
                
                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var newMessage = await _messageService.SendMessageAsync(currentUserId, messageDto.ReceiverId, messageDto.Content);
                
                // Also publish to Service Bus
                await _serviceBusService.SendMessageAsync(newMessage);
                
                return Ok(newMessage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {ReceiverId}", messageDto.ReceiverId);
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        /// <summary>
        /// Marks a message as read
        /// </summary>
        /// <param name="messageId">The message ID to mark as read</param>
        /// <returns>The updated message</returns>
        [HttpPut("read/{messageId}")]
        public async Task<ActionResult<Message>> MarkMessageAsRead(int messageId)
        {
            try
            {
                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var message = await _messageService.MarkMessageAsReadAsync(messageId, currentUserId);
                return Ok(message);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                return StatusCode(500, "An error occurred while updating the message");
            }
        }

        /// <summary>
        /// Gets the most recent messages for the current user
        /// </summary>
        /// <param name="count">Number of messages to retrieve (default 20)</param>
        /// <returns>List of recent messages</returns>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<Message>>> GetRecentMessages([FromQuery] int count = 20)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var recentMessages = await _messageService.GetRecentMessagesAsync(currentUserId, count);
                return Ok(recentMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent messages");
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Gets a list of users with whom the current user has active conversations
        /// </summary>
        /// <returns>List of users with conversation summaries</returns>
        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<object>>> GetConversations()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var conversations = await _messageService.GetConversationsAsync(currentUserId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversations");
                return StatusCode(500, "An error occurred while retrieving conversations");
            }
        }

        /// <summary>
        /// Gets messages by specific sender and receiver IDs
        /// </summary>
        /// <param name="senderId">The sender user ID</param>
        /// <param name="receiverId">The receiver user ID</param>
        /// <returns>List of messages</returns>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByUsers([FromQuery] string senderId, [FromQuery] string receiverId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _messageService.GetMessagesByUsersAsync(currentUserId, senderId, receiverId);
                return Ok(messages);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You can only view messages where you are either the sender or receiver");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages between sender {SenderId} and receiver {ReceiverId}", senderId, receiverId);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }
    }
} 