using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Models;
using Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarService _calendarService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            CalendarService calendarService,
            ILogger<CalendarController> logger
        )
        {
            _calendarService = calendarService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var events = await _calendarService.GetEventsAsync(userId, start, end);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var calendarEvent = await _calendarService.GetEventByIdAsync(id, userId);

            if (calendarEvent == null)
                return NotFound(new { message = "Event not found" });

            return Ok(calendarEvent);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CalendarEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var calendarEvent = new CalendarEvent
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsAllDay = dto.IsAllDay,
                    Color = dto.Color,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                };

                var createdEvent = await _calendarService.CreateEventAsync(calendarEvent);
                return CreatedAtAction(
                    nameof(GetEvent),
                    new { id = createdEvent.Id },
                    createdEvent
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating calendar event");
                return StatusCode(
                    500,
                    new { message = "An error occurred while creating the event" }
                );
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CalendarEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var calendarEvent = new CalendarEvent
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsAllDay = dto.IsAllDay,
                    Color = dto.Color,
                };

                var updatedEvent = await _calendarService.UpdateEventAsync(
                    id,
                    calendarEvent,
                    userId
                );

                if (updatedEvent == null)
                    return NotFound(new { message = "Event not found" });

                return Ok(updatedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating calendar event");
                return StatusCode(
                    500,
                    new { message = "An error occurred while updating the event" }
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _calendarService.DeleteEventAsync(id, userId);

                if (!success)
                    return NotFound(new { message = "Event not found" });

                return Ok(new { message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting calendar event");
                return StatusCode(
                    500,
                    new { message = "An error occurred while deleting the event" }
                );
            }
        }
    }

    public class CalendarEventDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public bool IsAllDay { get; set; }

        [StringLength(50)]
        public string Color { get; set; }
    }
}
