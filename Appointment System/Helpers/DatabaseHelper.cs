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
                FullName = "Test User",
                Address = "123 Test Street",
                ProfilePictureUrl = "/images/default.jpg"
            };
            dbContext.Users.Add(user);

            // Add a test service provider
            var provider = new ApplicationUser
            {
                Id = "test-provider-id",
                UserName = "provider@example.com",
                Email = "provider@example.com",
                FullName = "Test Provider",
                PhoneNumber = "1234567890",
                IsServiceProvider = true,
                Address = "456 Provider Avenue",
                ProfilePictureUrl = "/images/provider.jpg"
            };
            dbContext.Users.Add(provider);

            // Add a test service
            var service = new Service
            {
                Id = 1,
                Name = "Test Service",
                Description = "A test service for unit testing",
                Price = 100.00m,
                ProviderId = "test-provider-id",
                IsActive = true
            };
            dbContext.Services.Add(service);

            // Add a test template
            var template = new Template
            {
                Id = 1,
                Name = "Test Template",
                Description = "A test template for unit testing",
                ProviderId = "test-provider-id"
            };
            dbContext.Templates.Add(template);

            // Add a test day
            var day = new Day
            {
                Id = 1,
                TemplateId = 1,
                Index = 1, // Monday
                IsAvailable = true
            };
            dbContext.Days.Add(day);

            // Add a test segment
            var segment = new Segment
            {
                Id = 1,
                DayId = 1,
                TemplateId = 1,
                StartTime = TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                EndTime = TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0))
            };
            dbContext.Segments.Add(segment);

            // Add a test slot
            var slot = new Slot
            {
                Id = 1,
                ServiceId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                EndTime = TimeOnly.FromTimeSpan(new TimeSpan(10, 0, 0))
            };
            dbContext.Slots.Add(slot);

            // Add a test arrangement
            var arrangement = new Arrangement
            {
                Id = 1,
                ServiceId = 1,
                TemplateId = 1
            };
            dbContext.Arrangements.Add(arrangement);

            dbContext.SaveChanges();
        }
    }
} 