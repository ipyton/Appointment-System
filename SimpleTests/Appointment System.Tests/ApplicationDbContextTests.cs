using System;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Appointment_System.Tests
{
    public class ApplicationDbContextTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public ApplicationDbContextTests()
        {
            // Set up in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CanAddAndRetrieveUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var user = new ApplicationUser
            {
                Id = "test-user",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FullName = "Test User",
                Address = "123 Test St",
                IsServiceProvider = false
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var retrievedUser = await context.Users.FindAsync("test-user");
            Assert.NotNull(retrievedUser);
            Assert.Equal("testuser@example.com", retrievedUser.Email);
            Assert.Equal("Test User", retrievedUser.FullName);
        }

        [Fact]
        public async Task CanAddAndRetrieveService()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            
            // Add a provider first
            var provider = new ApplicationUser
            {
                Id = "provider-1",
                UserName = "provider@example.com",
                Email = "provider@example.com",
                IsServiceProvider = true
            };
            context.Users.Add(provider);
            
            // Create service
            var service = new Service
            {
                Name = "Test Service",
                Description = "A test service",
                Price = 100.00m,
                DurationMinutes = 60,
                IsActive = true,
                ProviderId = provider.Id,
                Provider = provider
            };

            // Act
            context.Services.Add(service);
            await context.SaveChangesAsync();

            // Assert
            var retrievedService = await context.Services
                .Include(s => s.Provider)
                .FirstOrDefaultAsync(s => s.Name == "Test Service");
                
            Assert.NotNull(retrievedService);
            Assert.Equal(100.00m, retrievedService.Price);
            Assert.Equal("provider-1", retrievedService.ProviderId);
            Assert.NotNull(retrievedService.Provider);
            Assert.Equal("provider@example.com", retrievedService.Provider.Email);
        }

        [Fact]
        public async Task CanAddAndRetrieveAppointment()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            
            // Add user
            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "user@example.com",
                Email = "user@example.com"
            };
            context.Users.Add(user);
            
            // Add provider
            var provider = new ApplicationUser
            {
                Id = "provider-1",
                UserName = "provider@example.com",
                Email = "provider@example.com",
                IsServiceProvider = true
            };
            context.Users.Add(provider);
            
            // Add service
            var service = new Service
            {
                Id = 1,
                Name = "Test Service",
                Description = "A test service",
                Price = 100.00m,
                DurationMinutes = 60,
                IsActive = true,
                ProviderId = provider.Id,
                Provider = provider
            };
            context.Services.Add(service);
            
            // Add bill
            var bill = new Bill
            {
                Id = 1,
                Amount = 100.00m,
                Tax = 10.00m,
                TotalAmount = 110.00m,
                Status = BillStatus.Pending
            };
            context.Bills.Add(bill);
            
            // Create appointment
            var appointment = new Appointment
            {
                UserId = user.Id,
                User = user,
                ServiceId = service.Id,
                Service = service,
                TemplateId = 1,
                SlotId = 1,
                DayId = 1,
                SegmentId = 1,
                Status = AppointmentStatus.Pending,
                BillId = bill.Id,
                Bill = bill,
                AppointmentDate = DateTime.Today.AddDays(1),
                StartTime = DateTime.Today.AddDays(1).AddHours(10),
                EndTime = DateTime.Today.AddDays(1).AddHours(11)
            };

            // Act
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Assert
            var retrievedAppointment = await context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Include(a => a.Bill)
                .FirstOrDefaultAsync(a => a.UserId == "user-1");
                
            Assert.NotNull(retrievedAppointment);
            Assert.Equal(AppointmentStatus.Pending, retrievedAppointment.Status);
            Assert.Equal("user-1", retrievedAppointment.UserId);
            Assert.Equal(1, retrievedAppointment.ServiceId);
            Assert.Equal(1, retrievedAppointment.BillId);
            Assert.NotNull(retrievedAppointment.User);
            Assert.NotNull(retrievedAppointment.Service);
            Assert.NotNull(retrievedAppointment.Bill);
        }

        [Fact]
        public async Task CanAddAndRetrieveTemplate()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            
            // Add provider
            var provider = new ApplicationUser
            {
                Id = "provider-1",
                UserName = "provider@example.com",
                Email = "provider@example.com",
                IsServiceProvider = true
            };
            context.Users.Add(provider);
            
            // Create template
            var template = new Template
            {
                Name = "Weekly Schedule",
                Description = "Standard weekly schedule",
                ProviderId = provider.Id,
                Provider = provider,
                Type = false,
                IsActive = true
            };

            // Act
            context.Templates.Add(template);
            await context.SaveChangesAsync();

            // Assert
            var retrievedTemplate = await context.Templates
                .Include(t => t.Provider)
                .FirstOrDefaultAsync(t => t.Name == "Weekly Schedule");
                
            Assert.NotNull(retrievedTemplate);
            Assert.Equal("Standard weekly schedule", retrievedTemplate.Description);
            Assert.Equal("provider-1", retrievedTemplate.ProviderId);
            Assert.NotNull(retrievedTemplate.Provider);
            Assert.Equal("provider@example.com", retrievedTemplate.Provider.Email);
        }

        [Fact]
        public async Task CanAddAndRetrieveTemplateWithDays()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            
            // Add provider
            var provider = new ApplicationUser
            {
                Id = "provider-1",
                UserName = "provider@example.com",
                Email = "provider@example.com",
                IsServiceProvider = true
            };
            context.Users.Add(provider);
            
            // Create template
            var template = new Template
            {
                Id = 1,
                Name = "Weekly Schedule",
                Description = "Standard weekly schedule",
                ProviderId = provider.Id,
                Provider = provider,
                Type = false,
                IsActive = true
            };
            context.Templates.Add(template);
            
            // Create days
            var days = new[]
            {
                new Day { TemplateId = 1, DayOfWeek = DayOfWeek.Monday, IsActive = true },
                new Day { TemplateId = 1, DayOfWeek = DayOfWeek.Wednesday, IsActive = true },
                new Day { TemplateId = 1, DayOfWeek = DayOfWeek.Friday, IsActive = true }
            };
            context.Days.AddRange(days);

            // Act
            await context.SaveChangesAsync();

            // Assert
            var retrievedTemplate = await context.Templates
                .Include(t => t.Days)
                .FirstOrDefaultAsync(t => t.Id == 1);
                
            Assert.NotNull(retrievedTemplate);
            Assert.NotNull(retrievedTemplate.Days);
            Assert.Equal(3, retrievedTemplate.Days.Count);
            Assert.Contains(retrievedTemplate.Days, d => d.DayOfWeek == DayOfWeek.Monday);
            Assert.Contains(retrievedTemplate.Days, d => d.DayOfWeek == DayOfWeek.Wednesday);
            Assert.Contains(retrievedTemplate.Days, d => d.DayOfWeek == DayOfWeek.Friday);
        }
    }
} 