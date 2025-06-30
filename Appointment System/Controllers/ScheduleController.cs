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

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(ApplicationDbContext context, ILogger<ScheduleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetServiceSchedules(int serviceId)
        {
            var schedules = await _context.ServiceSchedules
                .Where(s => s.ServiceId == serviceId)
                .Select(s => new {
                    s.Id,
                    s.ServiceId,
                    s.StartDate,
                    s.RepeatWeeks,
                    s.SlotDurationMinutes,
                    s.IsActive,
                    WeekDays = _context.WeeklyAvailabilities
                        .Where(wa => wa.ServiceScheduleId == s.Id)
                        .Select(wa => new {
                            wa.Id,
                            wa.DayOfWeek,
                            wa.IsAvailable,
                            TimeSlots = _context.TimeSlots
                                .Where(ts => ts.WeeklyAvailabilityId == wa.Id)
                                .Select(ts => new {
                                    ts.Id,
                                    ts.StartTime,
                                    ts.EndTime,
                                    ts.MaxConcurrentAppointments
                                })
                                .OrderBy(ts => ts.StartTime)
                                .ToList()
                        })
                        .OrderBy(wa => wa.DayOfWeek)
                        .ToList()
                })
                .ToListAsync();

            return Ok(schedules);
        }

        [Authorize]
        [HttpPost("service/{serviceId}")]
        public async Task<IActionResult> CreateServiceSchedule(int serviceId, [FromBody] ServiceScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if service exists
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            // Create schedule
            var schedule = new ServiceSchedule
            {
                ServiceId = serviceId,
                StartDate = dto.StartDate,
                RepeatWeeks = dto.RepeatWeeks,
                SlotDurationMinutes = dto.SlotDurationMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Return the created schedule
            return CreatedAtAction(nameof(GetServiceSchedules), new { serviceId }, schedule);
        }

        [Authorize]
        [HttpPost("day/{scheduleId}")]
        public async Task<IActionResult> AddWeekDay(int scheduleId, [FromBody] WeekDayDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if schedule exists
            var schedule = await _context.ServiceSchedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }
            
            // Check if this day already exists for this schedule
            var existingDay = await _context.WeeklyAvailabilities
                .FirstOrDefaultAsync(wa => wa.ServiceScheduleId == scheduleId && wa.DayOfWeek == dto.DayOfWeek);
                
            if (existingDay != null)
            {
                return BadRequest(new { message = $"Day {dto.DayOfWeek} already exists for this schedule" });
            }

            // Create weekly day availability
            var weekDay = new WeeklyAvailability
            {
                ServiceScheduleId = scheduleId,
                DayOfWeek = dto.DayOfWeek,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeeklyAvailabilities.Add(weekDay);
            await _context.SaveChangesAsync();

            // Return the created day
            return Ok(weekDay);
        }
        
        [Authorize]
        [HttpPost("timeslot/{weekDayId}")]
        public async Task<IActionResult> AddTimeSlot(int weekDayId, [FromBody] TimeSlotDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if weekly day exists
            var weekDay = await _context.WeeklyAvailabilities.FindAsync(weekDayId);
            if (weekDay == null)
            {
                return NotFound(new { message = "Weekly day not found" });
            }
            
            // Validate time range
            if (dto.EndTime <= dto.StartTime)
            {
                return BadRequest(new { message = "End time must be after start time" });
            }

            // Create time slot
            var timeSlot = new TimeSlot
            {
                WeeklyAvailabilityId = weekDayId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxConcurrentAppointments = dto.MaxConcurrentAppointments,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            // Return the created time slot
            return Ok(timeSlot);
        }
        
        [Authorize]
        [HttpPut("timeslot/{id}")]
        public async Task<IActionResult> UpdateTimeSlot(int id, [FromBody] TimeSlotDto dto)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null)
            {
                return NotFound(new { message = "Time slot not found" });
            }
            
            // Validate time range
            if (dto.EndTime <= dto.StartTime)
            {
                return BadRequest(new { message = "End time must be after start time" });
            }

            // Update properties
            timeSlot.StartTime = dto.StartTime;
            timeSlot.EndTime = dto.EndTime;
            timeSlot.MaxConcurrentAppointments = dto.MaxConcurrentAppointments;
            timeSlot.UpdatedAt = DateTime.UtcNow;

            _context.TimeSlots.Update(timeSlot);
            await _context.SaveChangesAsync();

            return Ok(timeSlot);
        }
        
        [Authorize]
        [HttpDelete("timeslot/{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null)
            {
                return NotFound(new { message = "Time slot not found" });
            }

            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time slot deleted" });
        }
        
        [Authorize]
        [HttpDelete("day/{id}")]
        public async Task<IActionResult> DeleteWeekDay(int id)
        {
            var weekDay = await _context.WeeklyAvailabilities.FindAsync(id);
            if (weekDay == null)
            {
                return NotFound(new { message = "Weekly day not found" });
            }

            _context.WeeklyAvailabilities.Remove(weekDay);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Weekly day and all associated time slots deleted" });
        }
        
        [Authorize]
        [HttpDelete("service-schedule/{id}")]
        public async Task<IActionResult> DeleteServiceSchedule(int id)
        {
            var schedule = await _context.ServiceSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Service schedule not found" });
            }

            _context.ServiceSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service schedule deleted" });
        }
    }
    
    public class ServiceScheduleDto
    {
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public int? RepeatWeeks { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
    }
    
    public class WeekDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
    }
    
    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxConcurrentAppointments { get; set; } = 1;
    }
} 