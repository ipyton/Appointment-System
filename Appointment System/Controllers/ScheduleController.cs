using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

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
            
            var templates = await _context.Templates
                .Where(t => t.ProviderId == providerId)
                .Include(t => t.Days)
                .ToListAsync();

            return Ok(templates);
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateDetails(int id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var template = await _context.Templates
                .Include(t => t.Days)
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
                Days = new List<Day>()
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
            
            var template = await _context.Templates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.ProviderId == providerId);
                
            if (template == null)
                return NotFound(new { message = "Template not found" });
                
            var day = new Day
            {
                Index = dto.Index,
                TemplateId = templateId
            };
            
            _context.Days.Add(day);
            await _context.SaveChangesAsync();
            
            return Ok(day);
        }

        [HttpPost("days/{dayId}/segments")]
        public async Task<IActionResult> AddSegmentToDay(int dayId, [FromBody] SegmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var day = await _context.Days
                .Include(d => d.Template)
                .FirstOrDefaultAsync(d => d.Id == dayId);
                
            if (day == null)
                return NotFound(new { message = "Day not found" });
                
            if (day.Template.ProviderId != providerId)
                return Forbid();
                
            if (dto.StartTime >= dto.EndTime)
                return BadRequest(new { message = "End time must be after start time" });
                
            var segment = new Segment
            {
                DayId = dayId,
                TemplateId = day.TemplateId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationForSingleSlot = dto.DurationForSingleSlot
            };
            
            _context.Segments.Add(segment);
            await _context.SaveChangesAsync();
            
            // Create slots for this segment
            var currentTime = segment.StartTime;
            while (currentTime.AddMinutes(segment.DurationForSingleSlot.TotalMinutes) <= segment.EndTime)
            {
                var slot = new Slot
                {
                    DayId = dayId,
                    duration = segment.DurationForSingleSlot
                };
                
                _context.Slots.Add(slot);
                currentTime = currentTime.AddMinutes(segment.DurationForSingleSlot.TotalMinutes);
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(segment);
        }

        [HttpPost("arrangements")]
        public async Task<IActionResult> CreateArrangement([FromBody] ArrangementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Check if service exists and belongs to the provider
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.ProviderId == providerId);
                
            if (service == null)
                return NotFound(new { message = "Service not found" });
                
            // Check if template exists and belongs to the provider
            var template = await _context.Templates
                .FirstOrDefaultAsync(t => t.Id == dto.TemplateId && t.ProviderId == providerId);
                
            if (template == null)
                return NotFound(new { message = "Template not found" });
                
            var arrangement = new Arrangement
            {
                ServiceId = dto.ServiceId,
                TemplateId = dto.TemplateId,
                StartDateTime = dto.StartDateTime,
                RepeatTimes = dto.RepeatTimes,
                RepeatInterval = dto.RepeatInterval
            };
            
            _context.Arrangements.Add(arrangement);
            await _context.SaveChangesAsync();
            
            return Ok(arrangement);
        }

        [HttpGet("arrangements")]
        public async Task<IActionResult> GetProviderArrangements()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var arrangements = await _context.Arrangements
                .Include(a => a.Service)
                .Include(a => a.Template)
                .Where(a => a.Service.ProviderId == providerId)
                .ToListAsync();
                
            return Ok(arrangements);
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetProviderAppointments([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var query = _context.Appointments
                .Include(a => a.Bill)
                .Where(a => a.Service.ProviderId == providerId);
                
            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);
                
            var appointments = await query.ToListAsync();
            
            return Ok(appointments);
        }

        [HttpPut("appointments/{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var appointment = await _context.Appointments
                .Include(a => a.Bill)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id && a.Service.ProviderId == providerId);
                
            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });
                
            appointment.Status = dto.Status;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            // Update bill status if needed
            if (dto.Status == AppointmentStatus.Completed && appointment.Bill.Status == BillStatus.Pending)
            {
                appointment.Bill.Status = BillStatus.Paid;
                appointment.Bill.UpdatedAt = DateTime.UtcNow;
            }
            else if (dto.Status == AppointmentStatus.Cancelled && appointment.Bill.Status == BillStatus.Pending)
            {
                appointment.Bill.Status = BillStatus.Cancelled;
                appointment.Bill.UpdatedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(appointment);
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