using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(ApplicationDbContext context, ILogger<ScheduleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetProviderTemplates()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var templates = await _context
                .Templates.Where(t => t.ProviderId == providerId)
                .Include(t => t.Days)
                .ToListAsync();

            return Ok(templates);
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateDetails(int id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var template = await _context
                .Templates.Include(t => t.Days)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProviderId == providerId);

            if (template == null)
                return NotFound(new { message = "Template not found" });

            return Ok(template);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] TemplateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var template = new Template
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                ProviderId = providerId,
                Days = new List<Day>(),
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTemplateDetails), new { id = template.Id }, template);
        }

        [HttpPost("templates/{templateId}/days")]
        public async Task<IActionResult> AddDayToTemplate(int templateId, [FromBody] DayDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var template = await _context.Templates.FirstOrDefaultAsync(t =>
                t.Id == templateId && t.ProviderId == providerId
            );

            if (template == null)
                return NotFound(new { message = "Template not found" });

            var day = new Day { Index = dto.Index, TemplateId = templateId };

            _context.Days.Add(day);
            await _context.SaveChangesAsync();

            return Ok(day);
        }

        // [HttpPost("days/{dayId}/segments")]
        // public async Task<IActionResult> AddSegmentToDay(int dayId, [FromBody] SegmentDto dto)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //     var day = await _context.Days
        //         .Include(d => d.Template)
        //         .FirstOrDefaultAsync(d => d.Id == dayId);

        //     if (day == null)
        //         return NotFound(new { message = "Day not found" });

        //     if (day.Template.ProviderId != providerId)
        //         return Forbid();

        //     if (dto.StartTime >= dto.EndTime)
        //         return BadRequest(new { message = "End time must be after start time" });

        //     var segment = new Segment
        //     {
        //         DayId = dayId,
        //         TemplateId = day.TemplateId,
        //         StartTime = dto.StartTime,
        //         EndTime = dto.EndTime,
        //         DurationForSingleSlot = dto.DurationForSingleSlot
        //     };

        //     _context.Segments.Add(segment);
        //     await _context.SaveChangesAsync();

        //     // Create slots for this segment
        //     var currentTime = segment.StartTime;
        //     while (currentTime.AddMinutes(segment.DurationForSingleSlot.TotalMinutes) <= segment.EndTime)
        //     {
        //         var slot = new Slot
        //         {
        //             DayId = dayId,
        //             duration = segment.DurationForSingleSlot
        //         };

        //         _context.Slots.Add(slot);
        //         currentTime = currentTime.AddMinutes(segment.DurationForSingleSlot.TotalMinutes);
        //     }

        //     await _context.SaveChangesAsync();

        //     return Ok(segment);
        // }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetProviderAppointments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            int providerId
        )
        {
            var query = _context.Appointments.Where(a => a.ProviderId == providerId);

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value);

            var appointments = await query.ToListAsync();

            return Ok(appointments);
        }
    }

    public class TemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool Type { get; set; }
    }

    public class DayDto
    {
        [Required]
        [Range(0, 6)]
        public int Index { get; set; }
    }

    public class SegmentDto
    {
        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required]
        public TimeSpan DurationForSingleSlot { get; set; }
    }

    public class ArrangementDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Range(1, 52)]
        public int RepeatTimes { get; set; } = 1;

        [Required]
        [Range(1, 7)]
        public int RepeatInterval { get; set; } = 1;
    }

    public class UpdateStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }
    }
}
