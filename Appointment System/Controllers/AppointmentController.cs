using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentClientService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;
        private readonly ApplicationDbContext _context;

        public AppointmentController(
            AppointmentClientService appointmentService,
            ApplicationDbContext context,
            ILogger<AppointmentController> logger
        )
        {
            _appointmentService = appointmentService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableServices()
        {
            var services = await _appointmentService.GetAvailableServicesAsync();
            return Ok(services);
        }

        [HttpGet("services/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceDetails(int id)
        {
            var service = await _appointmentService.GetServiceByIdAsync(id);

            if (service == null)
                return NotFound(new { message = "Service not found" });

            return Ok(service);
        }

        [HttpGet("templates/{id}/days")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTemplateDays(int id)
        {
            var days = await _context.Days.Where(d => d.TemplateId == id).ToListAsync();
            return Ok(days);
        }

        [HttpGet("days/{id}/segments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDaySegments(int id)
        {
            var segments = await _context.Segments.Where(s => s.DayId == id).ToListAsync();
            return Ok(segments);
        }

        [HttpGet("slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSlots(DateOnly date, int serviceId)
        {
            var slots = await _context.Slots
                .Where(s => s.Date == date && s.ServiceId == serviceId && s.IsAvailable)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
                
            return Ok(slots);
        }

        [HttpPost("book")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Find the slot
                var slot = await _context.Slots.FindAsync(dto.SlotId);
                if (slot == null)
                    return NotFound(new { message = "Slot not found" });
                    
                if (!slot.IsAvailable || slot.CurrentAppointmentCount >= slot.MaxConcurrentAppointments)
                    return BadRequest(new { message = "This slot is not available for booking" });

                // Create the appointment
                var appointment = new Appointment
                {
                    UserId = userId,
                    ServiceId = dto.ServiceId,
                    SlotId = dto.SlotId,
                    ProviderId = 1, // This should be set based on the service or slot
                    Status = AppointmentStatus.Pending,
                    Notes = dto.Notes,
                    SpecialRequests = "",
                    CreatedAt = DateTime.UtcNow,
                    ContactEmail = dto.ContactEmail,
                    ContactPhone = dto.ContactPhone
                };

                _context.Appointments.Add(appointment);
                
                // Update slot availability
                slot.CurrentAppointmentCount++;
                if (slot.CurrentAppointmentCount >= slot.MaxConcurrentAppointments)
                {
                    slot.IsAvailable = false;
                }
                
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetAppointmentDetails),
                    new { id = appointment.Id },
                    appointment
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment");
                return StatusCode(
                    500,
                    new { message = "An error occurred while booking the appointment" }
                );
            }
        }

        [HttpPost("pay")]
        [Authorize]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentDto paymentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Find the appointment with its slot
                var appointment = await _context.Appointments
                    .Include(a => a.Slot)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.Id == paymentDto.AppointmentId);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Process payment (in a real application, you would integrate with a payment gateway)
                // For now, we'll just update the appointment status
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.UtcNow;
                
                // Update payment information
                appointment.PaymentMethod = paymentDto.PaymentMethod;
                appointment.PaymentAmount = paymentDto.Amount;
                appointment.PaymentCurrency = paymentDto.Currency;
                appointment.PaymentDate = DateTime.UtcNow;
                
                // Add special requests if provided
                if (!string.IsNullOrEmpty(paymentDto.SpecialRequests))
                {
                    appointment.SpecialRequests = paymentDto.SpecialRequests;
                }
                
                // Convert slot date/time to DateTime for calendar event
                var slot = appointment.Slot;
                var startDateTime = new DateTime(
                    slot.Date.Year, 
                    slot.Date.Month, 
                    slot.Date.Day,
                    slot.StartTime.Hour,
                    slot.StartTime.Minute,
                    0);
                    
                var endDateTime = new DateTime(
                    slot.Date.Year, 
                    slot.Date.Month, 
                    slot.Date.Day,
                    slot.EndTime.Hour,
                    slot.EndTime.Minute,
                    0);

                // Add calendar event for the user
                var calendarEvent = new CalendarEvent
                {
                    Title = appointment.Service?.Name ?? "Appointment",
                    Description = appointment.Notes,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    IsAllDay = false,
                    Color = "#4CAF50", // Green color for confirmed appointments
                    UserId = userId,
                    AppointmentId = appointment.Id,
                    ServiceId = appointment.ServiceId,
                    ServiceName = appointment.Service?.Name ?? "Service",
                    CreatedAt = DateTime.UtcNow,
                };

                _context.CalendarEvents.Add(calendarEvent);
                await _context.SaveChangesAsync();

                return Ok(
                    new
                    {
                        success = true,
                        message = "Payment processed successfully",
                        appointmentId = appointment.Id,
                        calendarEventId = calendarEvent.Id,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(
                    500,
                    new { message = "An error occurred while processing the payment" }
                );
            }
        }

        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context
                .Appointments
                .Include(a => a.Slot)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            return Ok(appointment);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var appointment = await _context.Appointments
                    .Include(a => a.Slot)
                    .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Check if appointment can be cancelled
                if (appointment.Status == AppointmentStatus.Completed)
                    return BadRequest(new { message = "Cannot cancel a completed appointment" });

                // Update appointment status
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;
                
                // Update slot availability
                var slot = appointment.Slot;
                if (slot != null)
                {
                    slot.CurrentAppointmentCount--;
                    slot.IsAvailable = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment");
                return StatusCode(
                    500,
                    new { message = "An error occurred while cancelling the appointment" }
                );
            }
        }
    }

    public class BookAppointmentDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
        
        [EmailAddress]
        public string ContactEmail { get; set; }
        
        public string ContactPhone { get; set; }

    }

    public class PaymentDto
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        [StringLength(500)]
        public string SpecialRequests { get; set; }

        public CardDetailsDto CardDetails { get; set; }
    }

    public class ContactInfoDto
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }
    }

    public class CardDetailsDto
    {
        public string CardNumber { get; set; }

        public string CardName { get; set; }

        public string ExpiryDate { get; set; }

        public string CVV { get; set; }
    }

    public class ServiceDetailsDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; }

        public string ProviderId { get; set; }

        public string ProviderName { get; set; }
    }
}
