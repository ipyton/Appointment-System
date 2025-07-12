using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Controllers;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using AppointmentSystem.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppointmentSystem.Tests.Controllers
{
    public class AppointmentControllerTests
    {
        private readonly Mock<ILogger<AppointmentController>> _loggerMock;
        private readonly Mock<AppointmentClientService> _appointmentServiceMock;
        private readonly string _userId = "test-user-id";

        public AppointmentControllerTests()
        {
            _loggerMock = new Mock<ILogger<AppointmentController>>();
            _appointmentServiceMock = new Mock<AppointmentClientService>(
                MockBehavior.Loose,
                Mock.Of<ApplicationDbContext>(),
                Mock.Of<ILogger<AppointmentClientService>>()
            );
        }

        private AppointmentController SetupController(ApplicationDbContext dbContext = null)
        {
            var controller = new AppointmentController(
                dbContext != null
                    ? new AppointmentClientService(
                        dbContext,
                        Mock.Of<ILogger<AppointmentClientService>>()
                    )
                    : _appointmentServiceMock.Object,
                dbContext ?? Mock.Of<ApplicationDbContext>(),
                _loggerMock.Object
            );

            // Setup ClaimsPrincipal for authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
                new Claim(ClaimTypes.Name, "testuser@example.com"),
                new Claim(ClaimTypes.Role, "User"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Setup controller context
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            return controller;
        }

        [Fact]
        public async Task GetAvailableServices_ReturnsOkResult_WithServices()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.GetAvailableServices();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var services = Assert.IsAssignableFrom<IEnumerable<Service>>(okResult.Value);
            Assert.Single(services);
            Assert.Equal("Test Service", services.First().Name);
        }

        [Fact]
        public async Task GetServiceDetails_ReturnsOkResult_WhenServiceExists()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.GetServiceDetails(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var service = Assert.IsType<Service>(okResult.Value);
            Assert.Equal(1, service.Id);
            Assert.Equal("Test Service", service.Name);
        }

        [Fact]
        public async Task GetServiceDetails_ReturnsNotFound_WhenServiceDoesNotExist()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.GetServiceDetails(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BookAppointment_ReturnsCreatedAtAction_WhenValid()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            var dto = new BookAppointmentDto
            {
                ServiceId = 1,
                SlotId = 1,
                Notes = "Test appointment",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890"
            };

            // Act
            var result = await controller.BookAppointment(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var appointment = Assert.IsType<Appointment>(createdAtActionResult.Value);
            Assert.Equal(_userId, appointment.UserId);
            Assert.Equal(dto.ServiceId, appointment.ServiceId);
            Assert.Equal(dto.Notes, appointment.Notes);
            Assert.Equal(AppointmentStatus.Pending, appointment.Status);
        }

        [Fact]
        public async Task GetAppointmentsByUserId_ReturnsOkResult_WithUserAppointments()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Book an appointment first
            var dto = new BookAppointmentDto
            {
                ServiceId = 1,
                SlotId = 1,
                Notes = "Test appointment",
                ContactEmail = "test@example.com",
                ContactPhone = "123-456-7890"
            };
            await controller.BookAppointment(dto);

            // Act
            var result = await controller.GetAppointmentsByUserId();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<IEnumerable<Appointment>>(okResult.Value);
            Assert.Single(appointments);
            Assert.Equal(_userId, appointments.First().UserId);
        }
    }
}
