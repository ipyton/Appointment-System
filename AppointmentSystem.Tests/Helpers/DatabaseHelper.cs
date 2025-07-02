using System;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;

namespace AppointmentSystem.Tests.Helpers
{
    public static class DatabaseHelper
    {
        public static ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);
            
            // Seed the database with test data
            SeedDatabase(dbContext);
            
            return dbContext;
        }

        private static void SeedDatabase(ApplicationDbContext dbContext)
        {
            // Add a test user
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            dbContext.Users.Add(user);

            // Add a test provider
            var provider = new Provider
            {
                Id = 1,
                Name = "Test Provider",
                Email = "provider@example.com",
                PhoneNumber = "1234567890",
                IsActive = true
            };
            dbContext.Providers.Add(provider);

            // Add a test service
            var service = new Service
            {
                Id = 1,
                Name = "Test Service",
                Description = "A test service for unit testing",
                DurationMinutes = 60,
                Price = 100.00m,
                ProviderId = 1,
                Provider = provider,
                IsActive = true
            };
            dbContext.Services.Add(service);

            // Add a test template
            var template = new Template
            {
                Id = 1,
                Name = "Test Template",
                Description = "A test template for unit testing"
            };
            dbContext.Templates.Add(template);

            // Add a test day
            var day = new Day
            {
                Id = 1,
                TemplateId = 1,
                DayOfWeek = DayOfWeek.Monday,
                Template = template
            };
            dbContext.Days.Add(day);

            // Add a test segment
            var segment = new Segment
            {
                Id = 1,
                DayId = 1,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                Day = day
            };
            dbContext.Segments.Add(segment);

            // Add a test slot
            var slot = new Slot
            {
                Id = 1,
                DayId = 1,
                SegmentId = 1,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                Day = day,
                Segment = segment
            };
            dbContext.Slots.Add(slot);

            // Add a test arrangement
            var arrangement = new Arrangement
            {
                Id = 1,
                ServiceId = 1,
                TemplateId = 1,
                Service = service,
                Template = template
            };
            dbContext.Arrangements.Add(arrangement);

            dbContext.SaveChanges();
        }
    }
} 