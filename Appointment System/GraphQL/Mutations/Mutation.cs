using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using HotChocolate.Data;
using HotChocolate.AspNetCore.Authorization;
using Appointment_System.GraphQL.Attributes;

namespace Appointment_System.GraphQL.Mutations
{
    [GraphQLDescription("Mutations for the appointment system")]
    public class Mutation
    {
        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize(Roles = new string[] { "Admin", "ServiceProvider" })]
        public async Task<Service> AddService(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            string name,
            string description,
            decimal price,
            bool allowMultipleBookings = false)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException(new Error("User not authenticated", "UNAUTHENTICATED"));
            }
            
            var service = new Service
            {
                Name = name,
                Description = description,
                Price = price,
                ProviderId = userId,
                IsActive = true,
                allowMultipleBookings = allowMultipleBookings
            };

            context.Services.Add(service);
            await context.SaveChangesAsync();
            return service;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize(Roles = new string[] { "Admin", "ServiceProvider" })]
        public async Task<Service> UpdateService(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            int id,
            string name,
            string description,
            decimal price,
            bool allowMultipleBookings)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var service = await context.Services.FindAsync(id);
            
            if (service == null)
            {
                throw new GraphQLException(new Error("Service not found.", "SERVICE_NOT_FOUND"));
            }
            
            // Only allow the service owner or admin to update
            if (service.ProviderId != userId && !httpContextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                throw new GraphQLException(new Error("Not authorized to update this service.", "UNAUTHORIZED"));
            }

            service.Name = name;
            service.Description = description;
            service.Price = price;
            service.allowMultipleBookings = allowMultipleBookings;

            context.Services.Update(service);
            await context.SaveChangesAsync();
            return service;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize]
        public async Task<Appointment> CreateAppointment(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            DateTime startTime,
            int serviceId,
            AppointmentStatus status = AppointmentStatus.Pending)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException(new Error("User not authenticated", "UNAUTHENTICATED"));
            }
            
            var service = await context.Services.FindAsync(serviceId);
            if (service == null)
            {
                throw new GraphQLException(new Error("Service not found", "SERVICE_NOT_FOUND"));
            }

            var appointment = new Appointment
            {
                StartTime = startTime,
                EndTime = startTime.AddHours(1), // Default 1 hour appointment
                AppointmentDate = startTime.Date,
                UserId = userId,
                ServiceId = serviceId,
                ProviderId = 1, // This would need to be updated based on the service
                Status = status
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();
            
            return appointment;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize]
        public async Task<Appointment> UpdateAppointmentStatus(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            int id,
            AppointmentStatus status)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var appointment = await context.Appointments.FindAsync(id);
            
            if (appointment == null)
            {
                throw new GraphQLException(new Error("Appointment not found.", "APPOINTMENT_NOT_FOUND"));
            }
            
            var service = await context.Services.FindAsync(appointment.ServiceId);
            
            // Check if user is the appointment owner, service provider, or admin
            var isAdmin = httpContextAccessor.HttpContext.User.IsInRole("Admin");
            var isServiceProvider = service?.ProviderId == userId;
            var isAppointmentOwner = appointment.UserId == userId;
            
            if (!isAdmin && !isServiceProvider && !isAppointmentOwner)
            {
                throw new GraphQLException(new Error("Not authorized to update this appointment.", "UNAUTHORIZED"));
            }

            appointment.Status = status;
            context.Appointments.Update(appointment);
            await context.SaveChangesAsync();
            
            return appointment;
        }
    }
} 