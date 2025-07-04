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

namespace Appointment_System.GraphQL.Mutations
{
    [GraphQLDescription("Mutations for the appointment system")]
    public class Mutation
    {
        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize(Roles = new[] { "Admin", "ServiceProvider" })]
        public async Task<Service> AddService(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            string name,
            string description,
            decimal price,
            int durationMinutes)
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
                DurationMinutes = durationMinutes,
                ProviderId = userId,
                IsActive = true
            };

            context.Services.Add(service);
            await context.SaveChangesAsync();
            return service;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize(Roles = new[] { "Admin", "ServiceProvider" })]
        public async Task<Service> UpdateService(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            int id,
            string name,
            string description,
            decimal price,
            int durationMinutes)
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
            service.DurationMinutes = durationMinutes;

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
            string status = "Pending")
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException(new Error("User not authenticated", "UNAUTHENTICATED"));
            }
            
            var appointment = new Appointment
            {
                StartTime = startTime,
                UserId = userId,
                ServiceId = serviceId,
                Status = status
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();
            
            // Load related entities for the return value
            await context.Entry(appointment)
                .Reference(a => a.User)
                .LoadAsync();
                
            await context.Entry(appointment)
                .Reference(a => a.Service)
                .LoadAsync();
                
            return appointment;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [Authorize]
        public async Task<Appointment> UpdateAppointmentStatus(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            int id,
            string status)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var appointment = await context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (appointment == null)
            {
                throw new GraphQLException(new Error("Appointment not found.", "APPOINTMENT_NOT_FOUND"));
            }
            
            // Check if user is the appointment owner, service provider, or admin
            var isAdmin = httpContextAccessor.HttpContext.User.IsInRole("Admin");
            var isServiceProvider = appointment.Service.ProviderId == userId;
            var isAppointmentOwner = appointment.UserId == userId;
            
            if (!isAdmin && !isServiceProvider && !isAppointmentOwner)
            {
                throw new GraphQLException(new Error("Not authorized to update this appointment.", "UNAUTHORIZED"));
            }

            appointment.Status = status;
            context.Appointments.Update(appointment);
            await context.SaveChangesAsync();
            
            // Load related entities for the return value
            await context.Entry(appointment)
                .Reference(a => a.User)
                .LoadAsync();
                
            return appointment;
        }
    }
} 