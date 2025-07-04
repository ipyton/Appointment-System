using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Appointment_System.GraphQL.Queries
{
    [GraphQLDescription("Queries for the appointment system")]
    public class Query
    {
        [UseDbContext(typeof(ApplicationDbContext))]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = new[] { "Admin" })]
        [GraphQLDescription("Get all users in the system")]
        public IQueryable<ApplicationUser> GetUsers([ScopedService] ApplicationDbContext context)
        {
            return context.Users;
        }

        [UseDbContext(typeof(ApplicationDbContext))]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize]
        [GraphQLDescription("Get all active services")]
        public IQueryable<Service> GetServices([ScopedService] ApplicationDbContext context)
        {
            return context.Services.Where(s => s.IsActive);
        }
        
        [UseDbContext(typeof(ApplicationDbContext))]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize]
        [GraphQLDescription("Get all appointments")]
        public IQueryable<Appointment> GetAppointments([ScopedService] ApplicationDbContext context)
        {
            return context.Appointments.Include(a => a.User).Include(a => a.Service);
        }
        
        [UseDbContext(typeof(ApplicationDbContext))]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize]
        [GraphQLDescription("Get the current user's information")]
        public async Task<ApplicationUser> GetCurrentUser(
            [ScopedService] ApplicationDbContext context,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] UserManager<ApplicationUser> userManager)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException(new Error("User not authenticated", "UNAUTHENTICATED"));
            }
            
            var user = await userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                throw new GraphQLException(new Error("User not found", "USER_NOT_FOUND"));
            }
            
            return user;
        }
        
        [GraphQLDescription("Get a user by ID")]
        public async Task<ApplicationUser> GetUserById(
            [Service] ApplicationDbContext context,
            string id)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        
        [GraphQLDescription("Get a service by ID")]
        public async Task<Service> GetServiceById(
            [Service] ApplicationDbContext context,
            int id)
        {
            return await context.Services.FirstOrDefaultAsync(s => s.Id == id);
        }
        
        [GraphQLDescription("Get an appointment by ID")]
        public async Task<Appointment> GetAppointmentById(
            [Service] ApplicationDbContext context,
            int id)
        {
            return await context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        }
    }
} 