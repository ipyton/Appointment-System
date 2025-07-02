using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Appointment_System.Tests
{
    public class AppointmentClientServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<ILogger<AppointmentClientService>> _loggerMock;

        public AppointmentClientServiceTests()
        {
            // Set up in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _loggerMock = new Mock<ILogger<AppointmentClientService>>();
            
            // Seed the database
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                SeedDatabase(context);
            }
        }

        [Fact]
        public async Task GetAvailableServicesAsync_ReturnsActiveServices()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new AppointmentClientService(context, _loggerMock.Object);

            // Act
            var result = await service.GetAvailableServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // We expect 2 active services from our seed data
            Assert.All(result, s => Assert.True(s.IsActive));
        }

        [Fact]
        public async Task GetServiceByIdAsync_WithValidId_ReturnsService()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new AppointmentClientService(context, _loggerMock.Object);
            var serviceId = 1;

            // Act
            var result = await service.GetServiceByIdAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(serviceId, result.Id);
            Assert.Equal("Test Service 1", result.Name);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new AppointmentClientService(context, _loggerMock.Object);
            var invalidServiceId = 999;

            // Act
            var result = await service.GetServiceByIdAsync(invalidServiceId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserAppointmentsAsync_ReturnsUserAppointments()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new AppointmentClientService(context, _loggerMock.Object);
            var userId = "user1";

            // Act
            var result = await service.GetUserAppointmentsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // User1 has 2 appointments in our seed data
            Assert.All(result, a => Assert.Equal(userId, a.UserId));
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Add test users
            var user1 = new ApplicationUser
            {
                Id = "user1",
                UserName = "user1@example.com",
                Email = "user1@example.com"
            };
            
            var provider1 = new ApplicationUser
            {
                Id = "provider1",
                UserName = "provider1@example.com",
                Email = "provider1@example.com",
                IsServiceProvider = true
            };
            
            context.Users.AddRange(user1, provider1);
            
            // Add test services
            var services = new List<Service>
            {
                new Service
                {
                    Id = 1,
                    Name = "Test Service 1",
                    Description = "Description for test service 1",
                    Price = 100.00m,
                    DurationMinutes = 60,
                    IsActive = true,
                    ProviderId = provider1.Id,
                    Provider = provider1
                },
                new Service
                {
                    Id = 2,
                    Name = "Test Service 2",
                    Description = "Description for test service 2",
                    Price = 150.00m,
                    DurationMinutes = 90,
                    IsActive = true,
                    ProviderId = provider1.Id,
                    Provider = provider1
                },
                new Service
                {
                    Id = 3,
                    Name = "Inactive Service",
                    Description = "This service is not active",
                    Price = 200.00m,
                    DurationMinutes = 120,
                    IsActive = false,
                    ProviderId = provider1.Id,
                    Provider = provider1
                }
            };
            
            context.Services.AddRange(services);
            
            // Add test bills
            var bills = new List<Bill>
            {
                new Bill
                {
                    Id = 1,
                    Amount = 100.00m,
                    Tax = 10.00m,
                    TotalAmount = 110.00m,
                    Status = BillStatus.Paid
                },
                new Bill
                {
                    Id = 2,
                    Amount = 150.00m,
                    Tax = 15.00m,
                    TotalAmount = 165.00m,
                    Status = BillStatus.Pending
                }
            };
            
            context.Bills.AddRange(bills);
            
            // Add test appointments
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    UserId = user1.Id,
                    User = user1,
                    ServiceId = 1,
                    Service = services[0],
                    TemplateId = 1,
                    SlotId = 1,
                    DayId = 1,
                    SegmentId = 1,
                    Status = AppointmentStatus.Confirmed,
                    BillId = 1,
                    Bill = bills[0],
                    AppointmentDate = DateTime.Today.AddDays(1),
                    StartTime = DateTime.Today.AddDays(1).AddHours(10),
                    EndTime = DateTime.Today.AddDays(1).AddHours(11)
                },
                new Appointment
                {
                    Id = 2,
                    UserId = user1.Id,
                    User = user1,
                    ServiceId = 2,
                    Service = services[1],
                    TemplateId = 1,
                    SlotId = 2,
                    DayId = 1,
                    SegmentId = 1,
                    Status = AppointmentStatus.Pending,
                    BillId = 2,
                    Bill = bills[1],
                    AppointmentDate = DateTime.Today.AddDays(2),
                    StartTime = DateTime.Today.AddDays(2).AddHours(14),
                    EndTime = DateTime.Today.AddDays(2).AddHours(15.5)
                }
            };
            
            context.Appointments.AddRange(appointments);
            context.SaveChanges();
        }
    }
} 