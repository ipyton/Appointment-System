using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Controllers;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Appointment_System.Tests
{
    public class AppointmentControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<ILogger<AppointmentController>> _loggerMock;
        private readonly Mock<AppointmentClientService> _appointmentServiceMock;
        private readonly ApplicationDbContext _context;

        public AppointmentControllerTests()
        {
            // Set up in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(_dbContextOptions);
            
            // Seed the database
            SeedDatabase(_context);
            
            // Set up mocks
            _loggerMock = new Mock<ILogger<AppointmentController>>();
            var loggerServiceMock = new Mock<ILogger<AppointmentClientService>>();
            _appointmentServiceMock = new Mock<AppointmentClientService>(_context, loggerServiceMock.Object);
        }

        [Fact]
        public async Task GetAvailableServices_ReturnsOkWithServices()
        {
            // Arrange
            var controller = new AppointmentController(_appointmentServiceMock.Object, _context, _loggerMock.Object);
            
            // Act
            var result = await controller.GetAvailableServices();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var services = Assert.IsAssignableFrom<IEnumerable<Service>>(okResult.Value);
            Assert.NotEmpty(services);
        }

        [Fact]
        public async Task GetServiceDetails_WithValidId_ReturnsOkWithService()
        {
            // Arrange
            var controller = new AppointmentController(_appointmentServiceMock.Object, _context, _loggerMock.Object);
            var serviceId = 1;
            
            // Act
            var result = await controller.GetServiceDetails(serviceId);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var service = Assert.IsType<Service>(okResult.Value);
            Assert.Equal(serviceId, service.Id);
        }

        [Fact]
        public async Task GetServiceDetails_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new AppointmentController(_appointmentServiceMock.Object, _context, _loggerMock.Object);
            var invalidServiceId = 999;
            
            // Act
            var result = await controller.GetServiceDetails(invalidServiceId);
            
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BookAppointment_WithValidData_ReturnsOk()
        {
            // Arrange
            // Set up user claims for authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1"),
                new Claim(ClaimTypes.Name, "user1@example.com"),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            var controller = new AppointmentController(_appointmentServiceMock.Object, _context, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var dto = new BookAppointmentDto
            {
                ServiceId = 1,
                TemplateId = 1,
                SlotId = 1,
                DayId = 1,
                SegmentId = 1,
                Notes = "Test appointment"
            };

            // Set up the mock to return a successful booking
            var appointment = new Appointment
            {
                Id = 3,
                UserId = "user1",
                ServiceId = dto.ServiceId,
                TemplateId = dto.TemplateId,
                SlotId = dto.SlotId,
                DayId = dto.DayId,
                SegmentId = dto.SegmentId,
                Notes = dto.Notes,
                Status = AppointmentStatus.Pending
            };

            // Mock the service to simulate a successful booking
            _appointmentServiceMock.Setup(s => s.BookAppointmentAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), 
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(appointment);

            // Act
            var result = await controller.BookAppointment(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal(dto.ServiceId, returnedAppointment.ServiceId);
            Assert.Equal("user1", returnedAppointment.UserId);
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
                }
            };
            
            context.Services.AddRange(services);
            
            // Add test templates
            var templates = new List<Template>
            {
                new Template
                {
                    Id = 1,
                    Name = "Weekly Schedule",
                    Description = "Standard weekly schedule",
                    ProviderId = provider1.Id,
                    Provider = provider1,
                    Type = false,
                    IsActive = true
                }
            };
            
            context.Templates.AddRange(templates);
            
            // Add test days
            var days = new List<Day>
            {
                new Day
                {
                    Id = 1,
                    TemplateId = 1,
                    DayOfWeek = DayOfWeek.Monday,
                    IsActive = true
                }
            };
            
            context.Days.AddRange(days);
            
            // Add test segments
            var segments = new List<Segment>
            {
                new Segment
                {
                    Id = 1,
                    DayId = 1,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(12)
                }
            };
            
            context.Segments.AddRange(segments);
            
            // Add test slots
            var slots = new List<Slot>
            {
                new Slot
                {
                    Id = 1,
                    DayId = 1,
                    SegmentId = 1,
                    Duration = TimeSpan.FromMinutes(30),
                    StartTime = DateTime.Today.AddHours(9),
                    EndTime = DateTime.Today.AddHours(9.5),
                    IsAvailable = true
                },
                new Slot
                {
                    Id = 2,
                    DayId = 1,
                    SegmentId = 1,
                    Duration = TimeSpan.FromMinutes(30),
                    StartTime = DateTime.Today.AddHours(10),
                    EndTime = DateTime.Today.AddHours(10.5),
                    IsAvailable = true
                }
            };
            
            context.Slots.AddRange(slots);
            
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
                }
            };
            
            context.Bills.AddRange(bills);
            
            context.SaveChanges();
        }
    }
} 