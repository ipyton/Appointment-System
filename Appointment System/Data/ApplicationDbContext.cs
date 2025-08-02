using Appointment_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Appointment_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

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
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<StarredService> StarredServices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships for ApplicationUser
            builder
                .Entity<ApplicationUser>()
                .HasMany<Appointment>()
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Service to Arrangement relationship
            builder
                .Entity<Service>()
                .HasMany(s => s.Arrangements)
                .WithOne()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Arrangement to Template relationship (one-to-one)
            builder
                .Entity<Arrangement>()
                .HasOne<Template>()
                .WithMany()
                .HasForeignKey(a => a.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Template to Day relationship
            builder
                .Entity<Template>()
                .HasMany(t => t.Days)
                .WithOne()
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Day to Segment relationship
            builder
                .Entity<Day>()
                .HasMany(d => d.Segments)
                .WithOne()
                .HasForeignKey(s => s.DayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<Segment>()
                .HasOne<Template>()
                .WithMany()
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            // Slot to Appointment relationship (one-to-one)
            builder
                .Entity<Appointment>()
                .HasOne(a => a.Slot)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix the Bill to Appointment relationship
            // One Appointment can have one Bill
            builder
                .Entity<Appointment>()
                .HasOne<Bill>()
                .WithOne()
                .HasForeignKey<Bill>(b => b.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Message


            builder
                .Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Entity<Slot>()
                .HasOne<Service>()
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<TokenRecord>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(tr => tr.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure StarredService relationships
            builder
                .Entity<StarredService>()
                .HasOne(ss => ss.User)
                .WithMany()
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<StarredService>()
                .HasOne(ss => ss.Service)
                .WithMany()
                .HasForeignKey(ss => ss.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Add a unique constraint to prevent duplicate stars
            builder
                .Entity<StarredService>()
                .HasIndex(ss => new { ss.UserId, ss.ServiceId })
                .IsUnique();
        }
    }
}
