using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using Appointment_System.Models;
using Appointment.System.Tests.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Appointment.System.Tests.Services
{
    public class AppointmentClientServiceTests
    {
        [Fact]
        public async Task GetAvailableServicesAsync_ReturnsActiveServices()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);

            // Act
            var result = await service.GetAvailableServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Service", result.First().Name);
            Assert.True(result.All(s => s.IsActive));
        }

        [Fact]
        public async Task GetServiceByIdAsync_ReturnsCorrectService()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);

            // Act
            var result = await service.GetServiceByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Service", result.Name);
            Assert.NotNull(result.Provider);
            Assert.Equal("Test Provider", result.Provider.Name);
        }

        [Fact]
        public async Task GetServiceByIdAsync_ReturnsNullForInvalidId()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);

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
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);
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
            var bill = await dbContext.Bills.FirstOrDefaultAsync(b => b.AppointmentId == result.Id);
            Assert.NotNull(bill);
            Assert.Equal(100.00m, bill.Amount); // From our test data
            Assert.Equal(BillStatus.Pending, bill.Status);
        }

        [Fact]
        public async Task GetUserAppointmentsAsync_ReturnsUserAppointments()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);
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

        [Fact]
        public async Task CancelAppointmentAsync_CancelsAppointmentAndBill()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);
            var userId = "test-user-id";
            var serviceId = 1;
            var startTime = DateTime.Now.AddDays(2).Date.AddHours(10); // 2 days ahead to pass the 24-hour check

            // Create a test appointment
            var appointment = await service.BookAppointmentAsync(userId, serviceId, startTime);

            // Act
            var result = await service.CancelAppointmentAsync(appointment.Id, userId);

            // Assert
            Assert.True(result);

            // Verify appointment was cancelled
            var cancelledAppointment = await dbContext.Appointments.FindAsync(appointment.Id);
            Assert.NotNull(cancelledAppointment);
            Assert.Equal(AppointmentStatus.Cancelled, cancelledAppointment.Status);

            // Verify bill was cancelled
            var bills = await dbContext.Bills.Where(b => b.AppointmentId == appointment.Id).ToListAsync();
            Assert.NotEmpty(bills);
            Assert.All(bills, bill => Assert.Equal(BillStatus.Cancelled, bill.Status));
        }

        [Fact]
        public async Task CancelAppointmentAsync_ThrowsExceptionForTooLateCancel()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);
            var userId = "test-user-id";
            var serviceId = 1;
            var startTime = DateTime.Now.AddHours(12); // Only 12 hours ahead, less than the 24-hour requirement

            // Create a test appointment
            var appointment = await service.BookAppointmentAsync(userId, serviceId, startTime);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.CancelAppointmentAsync(appointment.Id, userId));
        }
    }
} 