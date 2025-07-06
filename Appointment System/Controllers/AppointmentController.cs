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
            var services = await _context
                .Services.Where(s => s.IsActive)
                .Include(s => s.Arrangements)
                .ToListAsync();
            return Ok(services);
        }

        [HttpGet("services/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceDetails(int id)
        {
            var service = await _context
                .Services.Include(s => s.Arrangements)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (service == null)
                return NotFound(new { message = "Service not found" });

            return Ok(service);
        }

        [HttpGet("services/{id}/templates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceTemplates(int id)
        {
            var templates = await _context.Arrangements.Where(a => a.ServiceId == id).ToListAsync();

            return Ok(templates);
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
            var slots = await _context
                .Slots.Where(s =>
                    s.Date == date && s.ServiceId == serviceId && s.IsAvailable == true
                )
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

                // Check if service exists
                var service = await _context.Services.FindAsync(dto.ServiceId);
                if (service == null)
                    return NotFound(new { message = "Service not found" });

                // Check if slot exists and is available
                var slot = await _context.Slots.FirstOrDefaultAsync(s => s.Id == dto.SlotId);

                if (slot == null)
                    return NotFound(new { message = "Slot not found" });

                if (slot.IsAvailable == false)
                    return BadRequest(new { message = "Slot is already booked" });

                // Mark the slot as unavailable
                slot.IsAvailable = false;

                await _context.SaveChangesAsync();

                // Create appointment
                var appointment = new Appointment
                {
                    UserId = userId,
                    ServiceId = dto.ServiceId,
                    SlotId = dto.SlotId,
                    Notes = dto.Notes,
                    Status = AppointmentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Appointments.Add(appointment);
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

                // Find the appointment
                var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                    a.Id == paymentDto.AppointmentId
                );

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Process payment (in a real application, you would integrate with a payment gateway)
                // For now, we'll just update the appointment status
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.UtcNow;

                // Add payment details
                appointment.PaymentMethod = paymentDto.PaymentMethod;
                appointment.PaymentAmount = paymentDto.Amount;
                appointment.PaymentCurrency = paymentDto.Currency;
                appointment.PaymentDate = DateTime.UtcNow;
                appointment.SpecialRequests = paymentDto.SpecialRequests;

                // Add contact information if provided
                if (paymentDto.ContactInfo != null)
                {
                    appointment.ContactEmail = paymentDto.ContactInfo.Email;
                    appointment.ContactPhone = paymentDto.ContactInfo.Phone;
                }

                // Add calendar event for the user
                var calendarEvent = new CalendarEvent
                {
                    Title = paymentDto.ServiceDetails.ServiceName,
                    Description = appointment.Notes,
                    StartTime = paymentDto.ServiceDetails.StartTime,
                    EndTime = paymentDto.ServiceDetails.EndTime,
                    IsAllDay = false,
                    Color = "#4CAF50", // Green color for confirmed appointments
                    UserId = userId,
                    AppointmentId = appointment.Id,
                    ServiceId = paymentDto.ServiceDetails.ServiceId,
                    ServiceName = paymentDto.ServiceDetails.ServiceName,
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
                .Appointments.Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                a.Id == id && a.UserId == userId
            );

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

                var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                    a.Id == id && a.UserId == userId
                );

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Check if appointment can be cancelled
                if (appointment.Status == AppointmentStatus.Completed)
                    return BadRequest(new { message = "Cannot cancel a completed appointment" });

                // Update appointment status
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;

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

        public ContactInfoDto ContactInfo { get; set; }

        [StringLength(500)]
        public string SpecialRequests { get; set; }

        public CardDetailsDto CardDetails { get; set; }

        [Required]
        public ServiceDetailsDto ServiceDetails { get; set; }
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

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
