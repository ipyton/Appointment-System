using System;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using AppointmentSystem.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppointmentSystem.Tests.Services
{
    public class AppointmentClientServiceTests
    {
        private readonly Mock<ILogger<AppointmentClientService>> _loggerMock;

        public AppointmentClientServiceTests()
        {
            _loggerMock = new Mock<ILogger<AppointmentClientService>>();
        }

        [Fact]
        public async Task GetAvailableServicesAsync_ReturnsActiveServices()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new AppointmentClientService(dbContext, _loggerMock.Object);

            // Act
            var result = await service.GetAvailableServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Service", result.First().Name);
            Assert.True(result.First().IsActive);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WithValidId_ReturnsService()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new AppointmentClientService(dbContext, _loggerMock.Object);

            // Act
            var result = await service.GetServiceByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Service", result.Name);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new AppointmentClientService(dbContext, _loggerMock.Object);

            // Act
            var result = await service.GetServiceByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task BookAppointmentAsync_CreatesAppointmentAndBill()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new AppointmentClientService(dbContext, _loggerMock.Object);
            var userId = "test-user-id";
            var serviceId = 1;
            var startTime = DateTime.Now.AddDays(1).Date.AddHours(10);

            // Act
            var result = await service.BookAppointmentAsync(userId, serviceId, startTime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(serviceId, result.ServiceId);
            Assert.Equal(startTime, result.StartTime);
            Assert.Equal(AppointmentStatus.Pending, result.Status);
            
            // Verify bill was created
            var bill = await dbContext.Bills.FindAsync(result.BillId);
            Assert.NotNull(bill);
            Assert.Equal(100.00m, bill.Amount); // From our test data
            Assert.Equal(BillStatus.Pending, bill.Status);
        }

        [Fact]
        public async Task GetUserAppointmentsAsync_ReturnsUserAppointments()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new AppointmentClientService(dbContext, _loggerMock.Object);
            var userId = "test-user-id";
            var serviceId = 1;
            var startTime = DateTime.Now.AddDays(1).Date.AddHours(10);

            // Create a test appointment
            await service.BookAppointmentAsync(userId, serviceId, startTime);

            // Act
            var result = await service.GetUserAppointmentsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userId, result.First().UserId);
            Assert.Equal(serviceId, result.First().ServiceId);
        }
    }
} 