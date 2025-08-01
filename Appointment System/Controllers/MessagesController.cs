using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Hubs;
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
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            ApplicationDbContext context,
            IHubContext<ChatHub> hubContext,
            ILogger<MessagesController> logger)
        {
            _context = context;
            _hubContext = hubContext;
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
                // Check if user exists
                var otherUser = await _context.Users.FindAsync(userId);
                if (otherUser == null)
                {
                    return NotFound("User not found");
                }

                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get messages between these two users (in both directions)
                var messages = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) || 
                                (m.SenderId == userId && m.ReceiverId == currentUserId))
                    .OrderByDescending(m => m.CreatedAt)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

                return Ok(messages);
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

                // Get unread messages where current user is the receiver
                var unreadMessages = await _context.Messages
                    .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                    .OrderByDescending(m => m.CreatedAt)
                    .Include(m => m.Sender)
                    .ToListAsync();

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
        [HttpPost("send/{receiverId}")]
        public async Task<ActionResult<Message>> SendMessage(string receiverId, [FromBody] string messageContent)
        {
            try
            {
                if (string.IsNullOrEmpty(messageContent))
                {
                    return BadRequest("Message content cannot be empty");
                }

                // Check if receiver exists
                var receiver = await _context.Users.FindAsync(receiverId);
                if (receiver == null)
                {
                    return NotFound("Recipient user not found");
                }

                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create a group name for these two users (alphabetically ordered for consistency)
                string groupName = currentUserId.CompareTo(receiverId) < 0 
                    ? $"chat-{currentUserId}-{receiverId}" 
                    : $"chat-{receiverId}-{currentUserId}";

                // Create new message
                var newMessage = new Message
                {
                    SenderId = currentUserId,
                    ReceiverId = receiverId,
                    Content = messageContent,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    GroupName = groupName
                };

                // Add to database
                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

                // Load the sender data for the response
                await _context.Entry(newMessage)
                    .Reference(m => m.Sender)
                    .LoadAsync();

                // Send to SignalR hub
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveMessage", newMessage);

                return Ok(newMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {ReceiverId}", receiverId);
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
                // Find message
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                {
                    return NotFound("Message not found");
                }

                // Get current user ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check if user is the receiver of this message
                if (message.ReceiverId != currentUserId)
                {
                    return Forbid("You don't have access to this message");
                }

                // Mark as read if not already read
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    // Notify via SignalR that message was read
                    await _hubContext.Clients
                        .Group(message.GroupName)
                        .SendAsync("MessageRead", messageId, currentUserId);
                }

                return Ok(message);
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

                // Get the most recent messages where current user is sender or receiver
                var recentMessages = await _context.Messages
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(count)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

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

                // Get users with whom the current user has exchanged messages
                var conversationPartners = await _context.Messages
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Distinct()
                    .ToListAsync();

                var conversations = new List<object>();

                foreach (var partnerId in conversationPartners)
                {
                    // Skip if somehow the user is matched with themselves
                    if (partnerId == currentUserId) continue;

                    // Get user info
                    var partner = await _context.Users.FindAsync(partnerId);
                    if (partner == null) continue;

                    // Get latest message
                    var latestMessage = await _context.Messages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == partnerId) || 
                                   (m.SenderId == partnerId && m.ReceiverId == currentUserId))
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefaultAsync();

                    // Count unread messages
                    var unreadCount = await _context.Messages
                        .CountAsync(m => m.SenderId == partnerId && 
                                        m.ReceiverId == currentUserId && 
                                        !m.IsRead);

                    // Add to results
                    conversations.Add(new
                    {
                        UserId = partnerId,
                        Name = partner.FullName,
                        LatestMessage = latestMessage?.Content,
                        LatestMessageTime = latestMessage?.CreatedAt,
                        UnreadCount = unreadCount
                    });
                }

                // Sort by latest message time
                return Ok(conversations.OrderByDescending(c => ((DateTime?)c.GetType().GetProperty("LatestMessageTime").GetValue(c)) ?? DateTime.MinValue));
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
                // Check if users exist
                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                {
                    return NotFound($"Sender with ID {senderId} not found");
                }
                
                var receiver = await _context.Users.FindAsync(receiverId);
                if (receiver == null)
                {
                    return NotFound($"Receiver with ID {receiverId} not found");
                }

                // Get current user ID for authorization
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // For security, ensure current user is either the sender or receiver
                if (currentUserId != senderId && currentUserId != receiverId)
                {
                    return Forbid("You can only view messages where you are either the sender or receiver");
                }

                // Get messages with the specified sender and receiver
                var messages = await _context.Messages
                    .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages between sender {SenderId} and receiver {ReceiverId}", senderId, receiverId);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }
    }
} 