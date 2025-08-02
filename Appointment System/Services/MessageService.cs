using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appointment_System.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetConversationAsync(string currentUserId, string otherUserId);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(string currentUserId);
        Task<Message> SendMessageAsync(string senderId, string receiverId, string messageContent);
        Task<Message> MarkMessageAsReadAsync(int messageId, string currentUserId);
        Task<IEnumerable<Message>> GetRecentMessagesAsync(string currentUserId, int count);
        Task<IEnumerable<object>> GetConversationsAsync(string currentUserId);
        Task<IEnumerable<Message>> GetMessagesByUsersAsync(string currentUserId, string senderId, string receiverId);
    }

    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            ApplicationDbContext context,
            IHubContext<ChatHub> hubContext,
            ILogger<MessageService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(string currentUserId, string otherUserId)
        {
            // Check if user exists
            var otherUser = await _context.Users.FindAsync(otherUserId);
            if (otherUser == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"User with ID {otherUserId} not found");
            }

            // Get messages between these two users (in both directions)
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();

            return messages;
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string currentUserId)
        {
            // Get unread messages where current user is the receiver
            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Sender)
                .ToListAsync();

            return unreadMessages;
        }

        public async Task<Message> SendMessageAsync(string senderId, string receiverId, string messageContent)
        {
            if (string.IsNullOrEmpty(messageContent))
            {
                throw new ArgumentException("Message content cannot be empty");
            }

            // Check if receiver exists
            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Recipient user with ID {receiverId} not found");
            }

            // Create a group name for these two users (alphabetically ordered for consistency)
            string groupName = senderId.CompareTo(receiverId) < 0
                ? $"chat-{senderId}-{receiverId}"
                : $"chat-{receiverId}-{senderId}";

            // Create new message
            var newMessage = new Message
            {
                SenderId = senderId,
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

            return newMessage;
        }

        public async Task<Message> MarkMessageAsReadAsync(int messageId, string currentUserId)
        {
            // Find message
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Message with ID {messageId} not found");
            }

            // Check if user is the receiver of this message
            if (message.ReceiverId != currentUserId)
            {
                throw new UnauthorizedAccessException("You don't have access to this message");
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

            return message;
        }

        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(string currentUserId, int count)
        {
            // Get the most recent messages where current user is sender or receiver
            var recentMessages = await _context.Messages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();

            return recentMessages;
        }

        public async Task<IEnumerable<object>> GetConversationsAsync(string currentUserId)
        {
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
            return conversations.OrderByDescending(c => ((DateTime?)c.GetType().GetProperty("LatestMessageTime").GetValue(c)) ?? DateTime.MinValue);
        }

        public async Task<IEnumerable<Message>> GetMessagesByUsersAsync(string currentUserId, string senderId, string receiverId)
        {
            // Check if users exist
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Sender with ID {senderId} not found");
            }
            
            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Receiver with ID {receiverId} not found");
            }

            // For security, ensure current user is either the sender or receiver
            if (currentUserId != senderId && currentUserId != receiverId)
            {
                throw new UnauthorizedAccessException("You can only view messages where you are either the sender or receiver");
            }

            // Get messages with the specified sender and receiver
            var messages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId)
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();

            return messages;
        }
    }
} 