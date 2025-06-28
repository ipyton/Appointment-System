using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Models;

namespace Appointment_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<ServiceAvailability> ServiceAvailabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure relationships for ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Services)
                .WithOne(s => s.Provider)
                .HasForeignKey(s => s.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.UserAppointments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Service
            builder.Entity<Service>()
                .HasMany(s => s.Availabilities)
                .WithOne(sa => sa.Service)
                .HasForeignKey(sa => sa.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Service>()
                .HasMany(s => s.Appointments)
                .WithOne(a => a.Service)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Appointment
            builder.Entity<Appointment>()
                .HasMany(a => a.Bills)
                .WithOne(b => b.Appointment)
                .HasForeignKey(b => b.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 