using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using Appointment_System.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly AppointmentProviderService _providerService;
        private readonly ILogger<EventsController> _logger;
        
        public EventsController(
            AppointmentProviderService providerService,
            ILogger<EventsController> logger)
        {
            _providerService = providerService;
            _logger = logger;
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint accessed");
            return Ok(new { message = "Test successful" });
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetProviderServices()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var services = await _providerService.GetProviderServicesAsync(providerId);
            return Ok(services);
        }

        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var service = new Service
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    ProviderId = providerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdService = await _providerService.CreateServiceAsync(service);
                return CreatedAtAction(nameof(GetServiceDetails), new { id = createdService.Id }, createdService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return StatusCode(500, new { message = "An error occurred while creating the service" });
            }
        }

        [HttpGet("services/{id}")]
        public async Task<IActionResult> GetServiceDetails(int id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var services = await _providerService.GetProviderServicesAsync(providerId);
            var service = services.Find(s => s.Id == id);
            
            if (service == null)
                return NotFound(new { message = "Service not found" });

            return Ok(service);
        }

        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var service = new Service
                {
                    Id = id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    ProviderId = providerId,
                    IsActive = dto.IsActive,
                    UpdatedAt = DateTime.UtcNow
                };

                var updatedService = await _providerService.UpdateServiceAsync(service);
                return Ok(updatedService);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service");
                return StatusCode(500, new { message = "An error occurred while updating the service" });
            }
        }

        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _providerService.DeleteServiceAsync(id, providerId);
                
                if (!success)
                    return NotFound(new { message = "Service not found" });

                return Ok(new { message = "Service deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service");
                return StatusCode(500, new { message = "An error occurred while deleting the service" });
            }
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Verify the service belongs to this provider
                var services = await _providerService.GetProviderServicesAsync(providerId);
                if (!services.Exists(s => s.Id == dto.ServiceId))
                    return BadRequest(new { message = "Service not found or does not belong to this provider" });

                // var schedule = new ServiceSchedule
                // {
                //     ServiceId = dto.ServiceId,
                //     StartDate = dto.StartDate,
                //     RepeatWeeks = dto.RepeatWeeks,
                //     SlotDurationMinutes = dto.SlotDurationMinutes,
                //     IsActive = true,
                //     CreatedAt = DateTime.UtcNow
                // };

                return Ok("true");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                return StatusCode(500, new { message = "An error occurred while creating the schedule" });
            }
        }

        [HttpPost("weekly-availability")]
        public async Task<IActionResult> AddWeeklyAvailability([FromBody] WeeklyAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // var availability = new WeeklyAvailability
                // {
                //     ServiceScheduleId = dto.ServiceScheduleId,
                //     DayOfWeek = dto.DayOfWeek,
                //     IsAvailable = true,
                //     CreatedAt = DateTime.UtcNow
                // };

                return Ok("true");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding weekly availability");
                return StatusCode(500, new { message = "An error occurred while adding weekly availability" });
            }
        }

        [HttpPost("time-slots")]
        public async Task<IActionResult> AddTimeSlot([FromBody] TimeSlotDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // var timeSlot = new TimeSlot
                // {
                //     WeeklyAvailabilityId = dto.WeeklyAvailabilityId,
                //     StartTime = dto.StartTime,
                //     EndTime = dto.EndTime,
                //     MaxConcurrentAppointments = dto.MaxConcurrentAppointments,
                //     CreatedAt = DateTime.UtcNow
                // };

                return Ok("true");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding time slot");
                return StatusCode(500, new { message = "An error occurred while adding time slot" });
            }
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetProviderAppointments([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, int providerId)
        {
            //var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _providerService.GetProviderAppointmentsAsync(providerId, startDate, endDate);
            return Ok(appointments);
        }

        [HttpGet("appointments/{id}")]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _providerService.GetAppointmentDetailsAsync(id, providerId);
            
            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            return Ok(appointment);
        }

        [HttpPut("appointments/{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _providerService.UpdateAppointmentStatusAsync(id, providerId, dto.Status);
                return Ok(appointment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status");
                return StatusCode(500, new { message = "An error occurred while updating appointment status" });
            }
        }

        [HttpPost("services/{id}/star")]
        public async Task<IActionResult> StarService(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var starredService = await _providerService.StarServiceAsync(id, userId);
                return Created($"/api/events/services/{id}/star", new { message = "Service starred successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starring service");
                return StatusCode(500, new { message = "An error occurred while starring the service" });
            }
        }

        [HttpDelete("services/{id}/star")]
        public async Task<IActionResult> UnstarService(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _providerService.UnstarServiceAsync(id, userId);
                
                if (!success)
                    return NotFound(new { message = "Service star not found" });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unstarring service");
                return StatusCode(500, new { message = "An error occurred while unstarring the service" });
            }
        }

        [HttpGet("services/starred")]
        public async Task<IActionResult> GetStarredServices()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var starredServices = await _providerService.GetStarredServicesAsync(userId);
                return Ok(starredServices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving starred services");
                return StatusCode(500, new { message = "An error occurred while retrieving starred services" });
            }
        }

        [HttpGet("services/{id}/is-starred")]
        public async Task<IActionResult> IsServiceStarred(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isStarred = await _providerService.IsServiceStarredAsync(id, userId);
                return Ok(new { isStarred });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if service is starred");
                return StatusCode(500, new { message = "An error occurred while checking if the service is starred" });
            }
        }
    }

    public class ServiceDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(5, 480)]
        public int DurationMinutes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class ScheduleDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        public int? RepeatWeeks { get; set; }

        [Required]
        [Range(5, 120)]
        public int SlotDurationMinutes { get; set; } = 30;
    }

    public class WeeklyAvailabilityDto
    {
        [Required]
        public int ServiceScheduleId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }
    }

    public class TimeSlotDto
    {
        [Required]
        public int WeeklyAvailabilityId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxConcurrentAppointments { get; set; } = 1;
    }
} 