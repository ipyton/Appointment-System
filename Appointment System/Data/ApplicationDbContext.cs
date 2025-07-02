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
        public DbSet<TokenRecord> Tokens { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Arrangement> Arrangements { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Day> Days { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Slot> Slots { get; set; }

        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships for ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Services)
                .WithOne(s => s.Provider)
                .HasForeignKey(s => s.ProviderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.UserAppointments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Service to Arrangement relationship
            builder.Entity<Service>()
                .HasMany<Arrangement>()
                .WithOne()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Arrangement to Template relationship (one-to-one)
            builder.Entity<Arrangement>()
                .HasOne<Template>()
                .WithMany()
                .HasForeignKey(a => a.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Template to Day relationship
            builder.Entity<Template>()
                .HasMany(t => t.Days)
                .WithOne()
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Day to Segment relationship
            builder.Entity<Day>()
                .HasMany<Segment>()
                .WithOne()
                .HasForeignKey(s => s.DayId)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder.Entity<Segment>()
            .HasOne<Template>()
            .WithMany()
            .HasForeignKey(s => s.TemplateId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Segment>()
            .HasOne<Day>()
            .WithMany()
            .HasForeignKey(s => s.DayId)
            .OnDelete(DeleteBehavior.NoAction);




            // Slot to Appointment relationship (one-to-one)
            builder.Entity<Slot>()
                .HasOne(s => s.Appointment)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.SlotId)
                .OnDelete(DeleteBehavior.Restrict);


            // Arrangement to Appointment relationship
            builder.Entity<Arrangement>()
                .HasMany<Appointment>()
                .WithOne()
                .HasForeignKey(a => a.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Appointment
            builder.Entity<Appointment>()
                .HasOne(a => a.Bill)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.BillId)
                .OnDelete(DeleteBehavior.NoAction);



            // Configure Message
            builder.Entity<Message>()
                .HasOne(m => m.Appointment)
                .WithMany()
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TokenRecord>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(tr => tr.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
} 