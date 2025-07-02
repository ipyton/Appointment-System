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
using Moq;
using Xunit;

namespace Appointment_System.Tests
{
    public class TemplateControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<TemplateService> _templateServiceMock;
        private readonly ApplicationDbContext _context;

        public TemplateControllerTests()
        {
            // Set up in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(_dbContextOptions);
            
            // Seed the database
            SeedDatabase(_context);
            
            // Set up mocks
            _templateServiceMock = new Mock<TemplateService>(_context);
        }

        [Fact]
        public async Task GetTemplates_WithProviderRole_ReturnsOkWithTemplates()
        {
            // Arrange
            // Set up user claims for authentication with Provider role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "provider1"),
                new Claim(ClaimTypes.Name, "provider1@example.com"),
                new Claim(ClaimTypes.Role, "Provider")
            }, "mock"));

            var controller = new TemplateController(_context, _templateServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Set up the mock to return templates
            var templates = new List<Template>
            {
                new Template { Id = 1, Name = "Template 1", ProviderId = "provider1" },
                new Template { Id = 2, Name = "Template 2", ProviderId = "provider1" }
            };
            _templateServiceMock.Setup(s => s.GetTemplatesForProviderAsync("provider1"))
                .ReturnsAsync(templates);

            // Act
            var result = await controller.GetTemplates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTemplates = Assert.IsAssignableFrom<IEnumerable<Template>>(okResult.Value);
            Assert.Equal(2, ((List<Template>)returnedTemplates).Count);
        }

        [Fact]
        public async Task GetTemplate_WithValidId_ReturnsOkWithTemplate()
        {
            // Arrange
            // Set up user claims for authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "provider1"),
                new Claim(ClaimTypes.Name, "provider1@example.com"),
                new Claim(ClaimTypes.Role, "Provider")
            }, "mock"));

            var controller = new TemplateController(_context, _templateServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var templateId = 1;
            var template = new Template
            {
                Id = templateId,
                Name = "Test Template",
                Description = "Test Description",
                ProviderId = "provider1",
                Days = new List<Day>()
            };
            
            _templateServiceMock.Setup(s => s.GetTemplateWithDetailsAsync(templateId))
                .ReturnsAsync(template);

            // Act
            var result = await controller.GetTemplate(templateId);

            // Assert
            var okResult = Assert.IsType<ActionResult<Template>>(result);
            var returnedTemplate = Assert.IsType<Template>(okResult.Value);
            Assert.Equal(templateId, returnedTemplate.Id);
            Assert.Equal("Test Template", returnedTemplate.Name);
        }

        [Fact]
        public async Task CreateTemplate_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            // Set up user claims for authentication with Provider role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "provider1"),
                new Claim(ClaimTypes.Name, "provider1@example.com"),
                new Claim(ClaimTypes.Role, "Provider")
            }, "mock"));

            var controller = new TemplateController(_context, _templateServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var newTemplate = new Template
            {
                Name = "New Template",
                Description = "New Template Description",
                Type = false,
                IsActive = true
            };

            var createdTemplate = new Template
            {
                Id = 3,
                Name = "New Template",
                Description = "New Template Description",
                ProviderId = "provider1",
                Type = false,
                IsActive = true
            };
            
            _templateServiceMock.Setup(s => s.CreateTemplateAsync(It.IsAny<Template>()))
                .ReturnsAsync(createdTemplate);

            // Act
            var result = await controller.CreateTemplate(newTemplate);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedTemplate = Assert.IsType<Template>(createdAtActionResult.Value);
            Assert.Equal(3, returnedTemplate.Id);
            Assert.Equal("New Template", returnedTemplate.Name);
            Assert.Equal("provider1", returnedTemplate.ProviderId);
        }

        [Fact]
        public async Task UpdateTemplate_WithValidData_ReturnsNoContent()
        {
            // Arrange
            // Set up user claims for authentication with Provider role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "provider1"),
                new Claim(ClaimTypes.Name, "provider1@example.com"),
                new Claim(ClaimTypes.Role, "Provider")
            }, "mock"));

            var controller = new TemplateController(_context, _templateServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var templateId = 1;
            var template = new Template
            {
                Id = templateId,
                Name = "Updated Template",
                Description = "Updated Description",
                ProviderId = "provider1"
            };

            // Mock the FindAsync to return a template owned by the provider
            _templateServiceMock.Setup(s => s.UpdateTemplateAsync(It.IsAny<Template>()))
                .ReturnsAsync(true);

            // Act
            var result = await controller.UpdateTemplate(templateId, template);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTemplate_WithValidId_ReturnsNoContent()
        {
            // Arrange
            // Set up user claims for authentication with Provider role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "provider1"),
                new Claim(ClaimTypes.Name, "provider1@example.com"),
                new Claim(ClaimTypes.Role, "Provider")
            }, "mock"));

            var controller = new TemplateController(_context, _templateServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var templateId = 1;
            
            // Mock the FindAsync to return a template owned by the provider
            var existingTemplate = new Template
            {
                Id = templateId,
                ProviderId = "provider1"
            };
            
            _templateServiceMock.Setup(s => s.DeleteTemplateAsync(templateId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteTemplate(templateId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Add test users/providers
            var provider1 = new ApplicationUser
            {
                Id = "provider1",
                UserName = "provider1@example.com",
                Email = "provider1@example.com",
                IsServiceProvider = true
            };
            
            var provider2 = new ApplicationUser
            {
                Id = "provider2",
                UserName = "provider2@example.com",
                Email = "provider2@example.com",
                IsServiceProvider = true
            };
            
            context.Users.AddRange(provider1, provider2);
            
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
                },
                new Template
                {
                    Id = 2,
                    Name = "Special Event Schedule",
                    Description = "Schedule for special events",
                    ProviderId = provider2.Id,
                    Provider = provider2,
                    Type = true,
                    IsActive = true
                }
            };
            
            context.Templates.AddRange(templates);
            
            // Add test services
            var services = new List<Service>
            {
                new Service
                {
                    Id = 1,
                    Name = "Test Service",
                    Description = "A test service",
                    Price = 100.00m,
                    DurationMinutes = 60,
                    IsActive = true,
                    IsPublic = true,
                    ProviderId = provider1.Id,
                    Provider = provider1
                }
            };
            
            context.Services.AddRange(services);
            
            context.SaveChanges();
        }
    }
} 