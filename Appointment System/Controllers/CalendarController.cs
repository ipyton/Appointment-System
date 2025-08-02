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
    [Route("[controller]")]
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

        /// <summary>
        /// Get calendar events by type (day, week, month)
        /// </summary>
        /// <param name="date">Reference date (defaults to today)</param>
        /// <param name="type">View type: day, week, month (defaults to month)</param>
        /// <returns>List of calendar events for the specified view</returns>
        [HttpGet("get")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime? date,
            [FromQuery] string type = "month"
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            DateTime start, end;

            // Default to current date if not specified
            date ??= DateTime.Today;

            // Normalize view parameter
            type = type?.ToLower() ?? "month";

            switch (type)
            {
                case "day":
                    // For day view, get events for the specific day only
                    start = date.Value.Date;
                    end = start.AddDays(1).AddSeconds(-1);
                    break;

                case "week":
                    // For week view, get events for 7 days starting from the specified date
                    start = date.Value.Date;
                    end = start.AddDays(7).AddSeconds(-1);
                    break;

                case "month":
                default:
                    // For month view, get events for the entire month containing the specified date
                    start = new DateTime(date.Value.Year, date.Value.Month, 1);
                    end = start.AddMonths(1).AddSeconds(-1);
                    break;
            }

            _logger.LogInformation(
                "Fetching {View} calendar events for user {UserId} from {Start} to {End}",
                type,
                userId,
                start,
                end
            );

            var events = await _calendarService.GetEventsAsync(userId, start, end);
            return Ok(
                new
                {
                    type = type,
                    startDate = start,
                    endDate = end,
                    totalEvents = events.Count,
                    events = events,
                }
            );
        }

        /// <summary>
        /// Get calendar events for a custom date range
        /// </summary>
        /// <param name="start">Start date (required)</param>
        /// <param name="end">End date (required)</param>
        /// <returns>List of calendar events for the specified date range</returns>
        [HttpGet("range")]
        public async Task<IActionResult> GetEventsByDateRange(
            [FromQuery, Required] DateTime start,
            [FromQuery, Required] DateTime end
        )
        {
            if (end < start)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Fetching custom range calendar events for user {UserId} from {Start} to {End}",
                userId,
                start,
                end
            );

            var events = await _calendarService.GetEventsAsync(userId, start, end);
            return Ok(
                new
                {
                    startDate = start,
                    endDate = end,
                    totalEvents = events.Count,
                    events = events,
                }
            );
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

    public class CalendarEventDto : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "End time must be after start time",
                    new[] { nameof(EndTime) }
                );
            }
        }
    }
}
