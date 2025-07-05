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
    public class AppointmentProviderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentProviderService> _logger;
        private readonly SearchIndexingEventHandler _searchIndexingHandler;

        public AppointmentProviderService(
            ApplicationDbContext context, 
            ILogger<AppointmentProviderService> logger,
            SearchIndexingEventHandler searchIndexingHandler)
        {
            _context = context;
            _logger = logger;
            _searchIndexingHandler = searchIndexingHandler;
        }

        /// <summary>
        /// Get all services for a provider
        /// </summary>
        public async Task<List<Service>> GetProviderServicesAsync(string providerId)
        {
            return await _context.Services
                .Where(s => s.ProviderId == providerId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new service
        /// </summary>
        public async Task<Service> CreateServiceAsync(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            
            // Index the service in search
            await _searchIndexingHandler.ServiceCreatedOrUpdatedAsync(service);
            
            _logger.LogInformation("Service created: {ServiceId} by provider {ProviderId}", service.Id, service.ProviderId);
            return service;
        }

        /// <summary>
        /// Update an existing service
        /// </summary>
        public async Task<Service> UpdateServiceAsync(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var existingService = await _context.Services.FindAsync(service.Id);
            if (existingService == null)
                throw new ArgumentException($"Service with ID {service.Id} not found");

            if (existingService.ProviderId != service.ProviderId)
                throw new UnauthorizedAccessException("You are not authorized to update this service");

            // Update properties
            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.Price = service.Price;
            existingService.UpdatedAt = DateTime.UtcNow;

            _context.Services.Update(existingService);
            await _context.SaveChangesAsync();
            
            // Index the updated service in search
            await _searchIndexingHandler.ServiceCreatedOrUpdatedAsync(existingService);
            
            _logger.LogInformation("Service updated: {ServiceId} by provider {ProviderId}", service.Id, service.ProviderId);
            return existingService;
        }

        /// <summary>
        /// Delete a service
        /// </summary>
        public async Task<bool> DeleteServiceAsync(int serviceId, string providerId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
                return false;

            if (service.ProviderId != providerId)
                throw new UnauthorizedAccessException("You are not authorized to delete this service");

            // Check if there are any future appointments for this service
            var hasFutureAppointments = await _context.Appointments
                .AnyAsync(a => a.ServiceId == serviceId && 
                              a.StartTime > DateTime.Now && 
                              a.Status != AppointmentStatus.Cancelled);

            if (hasFutureAppointments)
                throw new InvalidOperationException("Cannot delete a service with future appointments");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            
            // Remove the service from search index
            await _searchIndexingHandler.ServiceDeletedAsync(serviceId);
            
            _logger.LogInformation("Service deleted: {ServiceId} by provider {ProviderId}", serviceId, providerId);
            return true;
        }

        
        /// <summary>
        /// Get provider's appointments
        /// </summary>
        public async Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Appointments
                .Where(a => a.ProviderId == providerId);

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value.Date);

            return await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get appointment details
        /// </summary>
        public async Task<Appointment> GetAppointmentDetailsAsync(int appointmentId, string providerId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId );
        }

        /// <summary>
        /// Update appointment status
        /// </summary>
        public async Task<Appointment> UpdateAppointmentStatusAsync(int appointmentId, string providerId, AppointmentStatus status)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                throw new ArgumentException($"Appointment with ID {appointmentId} not found or you are not authorized");

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Update bill status if needed
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.AppointmentId == appointmentId);
                
            if (bill != null)
            {
                if (status == AppointmentStatus.Completed && bill.Status == BillStatus.Pending)
                {
                    bill.Status = BillStatus.Paid;
                    bill.PaidAt = DateTime.UtcNow;
                    bill.UpdatedAt = DateTime.UtcNow;
                }
                else if (status == AppointmentStatus.Cancelled && bill.Status == BillStatus.Pending)
                {
                    bill.Status = BillStatus.Cancelled;
                    bill.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Appointment status updated: {AppointmentId} to {Status} by provider {ProviderId}", 
                appointmentId, status, providerId);
                
            return appointment;
        }
    }
}