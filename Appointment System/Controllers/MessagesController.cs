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

        // GET: api/Messages/appointment/{appointmentId}
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByAppointment(int appointmentId)
        {
            // Verify the appointment exists
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound("Appointment not found");
            }

            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the user is part of the appointment (either the user or the service provider)
            if (appointment.UserId != userId && appointment.Service?.ProviderId != userId)
            {
                return Forbid("You don't have permission to view these messages");
            }

            // Get messages for the appointment
            var messages = await _context.Messages
                .Where(m => m.AppointmentId == appointmentId)
                .OrderBy(m => m.CreatedAt)
                .Include(m => m.Sender)
                .ToListAsync();

            // Mark unread messages as read if the current user is the recipient
            var unreadMessages = messages
                .Where(m => !m.IsRead && m.SenderId != userId)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return messages;
        }

        // POST: api/Messages
        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                return BadRequest("Message content cannot be empty");
            }

            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            message.SenderId = userId;
            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;

            // Verify the appointment exists
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == message.AppointmentId);
                
            if (appointment == null)
            {
                return NotFound("Appointment not found");
            }

            // Check if the user is part of the appointment (either the user or the service provider)
            if (appointment.UserId != userId && appointment.Service?.ProviderId != userId)
            {
                return Forbid("You don't have permission to send messages to this appointment");
            }

            // Determine the recipient
            string recipientId = appointment.UserId == userId 
                ? appointment.Service?.ProviderId ?? string.Empty
                : appointment.UserId;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Include sender information
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            // Send the message through SignalR
            await _hubContext.Clients.User(recipientId).SendAsync("ReceiveMessage", message);

            return CreatedAtAction("GetMessagesByAppointment", new { appointmentId = message.AppointmentId }, message);
        }

        // GET: api/Messages/unread
        [HttpGet("unread")]
        public async Task<ActionResult<int>> GetUnreadMessageCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get appointments where the user is either the client or provider
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.UserId == userId || a.Service.ProviderId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            // Count unread messages for these appointments where the user is not the sender
            var unreadCount = await _context.Messages
                .Where(m => appointments.Contains(m.AppointmentId) && 
                       m.SenderId != userId && 
                       !m.IsRead)
                .CountAsync();

            return unreadCount;
        }
    }
} 