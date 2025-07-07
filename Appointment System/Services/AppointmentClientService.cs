using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.Extensions.Logging;

namespace Appointment_System.Services
{
    public class AppointmentClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentClientService> _logger;

        public AppointmentClientService(ApplicationDbContext context, ILogger<AppointmentClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all available services
        /// </summary>
        public async Task<List<Service>> GetAvailableServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .Include(s => s.Arrangements)
                .ToListAsync();
        }

        /// <summary>
        /// Get service details by ID
        /// </summary>
        public async Task<Service> GetServiceByIdAsync(int serviceId)
        {
            return await _context.Services
                .Include(s => s.Arrangements)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);
        }

        /// <summary>
        /// Get available time slots for a service on a specific date
        /// </summary>
        public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(int serviceId, DateTime date)
        {
            var service = await _context.Services
                .Include(s => s.Arrangements)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);

            if (service == null)
                return new List<DateTime>();

            // Get all active schedules for this service that include the requested date
            // var activeSchedules = service.Arrangements
            //     .Where(ss => ss.IsActive && ss.StartTime <= date && 
            //         (!ss.RepeatTimes || 
            //          ss.StartTime.AddDays(ss.RepeatTimes * 7) >= date))
            //     .ToList();

            // var availableSlots = new List<DateTime>();
            
            // foreach (var schedule in activeSchedules)
            // {
            //     // Get weekly availabilities for the requested day of week
            //     var weeklyAvailabilities = schedule.WeeklyAvailabilities
            //         .Where(wa => wa.DayOfWeek == date.DayOfWeek && wa.IsAvailable)
            //         .ToList();

            //     foreach (var availability in weeklyAvailabilities)
            //     {
            //         // Get all time slots for this day
            //         foreach (var slot in availability.TimeSlots)
            //         {
            //             // Create DateTime objects for each slot on the requested date
            //             var slotStart = date.Date.Add(slot.StartTime);
            //             var slotEnd = date.Date.Add(slot.EndTime);

            //             // Check if this slot is already fully booked
            //             var existingAppointmentsCount = await _context.Appointments
            //                 .CountAsync(a => a.ServiceId == serviceId && 
            //                                 a.StartTime <= slotEnd && 
            //                                 a.EndTime >= slotStart &&
            //                                 a.Status != AppointmentStatus.Cancelled);

            //             if (existingAppointmentsCount < slot.MaxConcurrentAppointments)
            //             {
            //                 // Add slot start time to available slots
            //                 availableSlots.Add(slotStart);
            //             }
            //         }
            //     }
            // }

            return new List<DateTime>();
        }

        /// <summary>
        /// Book an appointment
        /// </summary>
        public async Task<Appointment> BookAppointmentAsync(string userId, int serviceId, int slotId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
                throw new ArgumentException("Service not found");
                
            var slot = await _context.Slots.FindAsync(slotId);
            if (slot == null)
                throw new ArgumentException("Slot not found");
                
            if (!slot.IsAvailable || slot.CurrentAppointmentCount >= slot.MaxConcurrentAppointments)
                throw new InvalidOperationException("This slot is not available for booking");

            // Create the appointment
            var appointment = new Appointment
            {
                UserId = userId,
                ServiceId = serviceId,
                SlotId = slotId,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            
            // Update slot availability
            slot.CurrentAppointmentCount++;
            if (slot.CurrentAppointmentCount >= slot.MaxConcurrentAppointments)
            {
                slot.IsAvailable = false;
            }
            
            await _context.SaveChangesAsync();

            // // Create a bill for the appointment
            // var bill = new Bill
            // {
            //     AppointmentId = appointment.Id,
            //     Amount = service.Price,
            //     Tax = service.Price * 0.1m, // Assuming 10% tax
            //     TotalAmount = service.Price * 1.1m,
            //     Status = BillStatus.Pending,
            //     CreatedAt = DateTime.UtcNow
            // };

            // _context.Bills.Add(bill);
            // await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment booked: {AppointmentId} for user {UserId}", appointment.Id, userId);
            return appointment;
        }

        /// <summary>
        /// Get appointment details
        /// </summary>
        public async Task<Appointment> GetAppointmentDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        /// <summary>
        /// Cancel an appointment
        /// </summary>
        public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
                return false;

            // Get the slot start time
            var slot = appointment.Slot;
            var startDateTime = new DateTime(
                slot.Date.Year,
                slot.Date.Month,
                slot.Date.Day,
                slot.StartTime.Hour,
                slot.StartTime.Minute,
                0);

            // Check if the appointment can be cancelled (e.g., not too close to start time)
            if (startDateTime <= DateTime.Now.AddHours(24))
                throw new InvalidOperationException("Appointments must be cancelled at least 24 hours in advance");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            // Update slot availability
            if (slot != null)
            {
                slot.CurrentAppointmentCount--;
                slot.IsAvailable = true;
            }

            // Update associated bill
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.AppointmentId == appointmentId);
                
            if (bill != null)
            {
                bill.Status = BillStatus.Cancelled;
                bill.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Appointment cancelled: {AppointmentId} by user {UserId}", appointmentId, userId);
            return true;
        }
    }
}