using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Appointment_System.Data;
using Appointment_System.Models;

namespace Appointment_System.Services
{
    public class CalendarService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalendarService> _logger;

        public CalendarService(ApplicationDbContext context, ILogger<CalendarService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all calendar events for a specific user within a date range
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="start">Start date (optional)</param>
        /// <param name="end">End date (optional)</param>
        /// <returns>List of calendar events</returns>
        public async Task<List<CalendarEvent>> GetEventsAsync(string userId, DateTime? start = null, DateTime? end = null)
        {
            var query = _context.CalendarEvents.Where(e => e.UserId == userId);

            if (start.HasValue)
            {
                query = query.Where(e => e.EndTime >= start.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(e => e.StartTime <= end.Value);
            }

            return await query
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific calendar event by ID
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="userId">The user ID (for authorization)</param>
        /// <returns>The calendar event or null if not found</returns>
        public async Task<CalendarEvent> GetEventByIdAsync(int eventId, string userId)
        {
            return await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == userId);
        }

        /// <summary>
        /// Create a new calendar event
        /// </summary>
        /// <param name="calendarEvent">The event to create</param>
        /// <returns>The created event</returns>
        public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
        {
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Calendar event created: {EventId} by user {UserId}", 
                calendarEvent.Id, calendarEvent.UserId);
                
            return calendarEvent;
        }

        /// <summary>
        /// Update an existing calendar event
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="updatedEvent">The updated event data</param>
        /// <param name="userId">The user ID (for authorization)</param>
        /// <returns>The updated event or null if not found</returns>
        public async Task<CalendarEvent> UpdateEventAsync(int eventId, CalendarEvent updatedEvent, string userId)
        {
            var existingEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == userId);

            if (existingEvent == null)
            {
                return null;
            }

            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.StartTime = updatedEvent.StartTime;
            existingEvent.EndTime = updatedEvent.EndTime;
            existingEvent.IsAllDay = updatedEvent.IsAllDay;
            existingEvent.Color = updatedEvent.Color;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Calendar event updated: {EventId} by user {UserId}", 
                existingEvent.Id, existingEvent.UserId);
                
            return existingEvent;
        }

        /// <summary>
        /// Delete a calendar event
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="userId">The user ID (for authorization)</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteEventAsync(int eventId, string userId)
        {
            var calendarEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == userId);

            if (calendarEvent == null)
            {
                return false;
            }

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Calendar event deleted: {EventId} by user {UserId}", 
                eventId, userId);
                
            return true;
        }
        
        /// <summary>
        /// Create calendar events from appointments
        /// </summary>
        /// <param name="appointment">The appointment to create an event from</param>
        /// <returns>The created calendar event</returns>
        public async Task<CalendarEvent> CreateEventFromAppointmentAsync(Appointment appointment)
        {
            var service = await _context.Services.FindAsync(appointment.ServiceId);
            
            if (service == null)
            {
                throw new ArgumentException("Service not found");
            }
            
            var calendarEvent = new CalendarEvent
            {
                Title = service.Name,
                Description = $"Appointment: {service.Description}",
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                IsAllDay = false,
                Color = "#4285F4", // Default blue color
                UserId = appointment.UserId,
                AppointmentId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            return await CreateEventAsync(calendarEvent);
        }
    }
} 