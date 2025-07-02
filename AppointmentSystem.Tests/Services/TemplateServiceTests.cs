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
    public class TemplateServiceTests
    {
        private readonly Mock<ILogger<TemplateService>> _loggerMock;

        public TemplateServiceTests()
        {
            _loggerMock = new Mock<ILogger<TemplateService>>();
        }

        [Fact]
        public async Task GetTemplateByIdAsync_WithValidId_ReturnsTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);

            // Act
            var result = await service.GetTemplateByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Template", result.Name);
        }

        [Fact]
        public async Task GetTemplateByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);

            // Act
            var result = await service.GetTemplateByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProviderTemplatesAsync_ReturnsTemplatesForProvider()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);
            var providerId = "test-provider-id";

            // Act
            var result = await service.GetProviderTemplatesAsync(providerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Template", result.First().Name);
            Assert.Equal(providerId, result.First().ProviderId);
        }

        [Fact]
        public async Task CreateTemplateAsync_CreatesNewTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);
            var providerId = "test-provider-id";
            
            var template = new Template
            {
                Name = "New Test Template",
                Description = "A new template for testing",
                ProviderId = providerId,
                IsActive = true,
                Type = false,
                BookingWindowDays = 14
            };

            // Act
            var result = await service.CreateTemplateAsync(template);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Test Template", result.Name);
            Assert.Equal(providerId, result.ProviderId);
            
            // Verify it was added to the database
            var savedTemplate = await dbContext.Templates.FindAsync(result.Id);
            Assert.NotNull(savedTemplate);
            Assert.Equal("New Test Template", savedTemplate.Name);
        }

        [Fact]
        public async Task UpdateTemplateAsync_UpdatesExistingTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);
            var templateId = 1;
            
            // Get existing template
            var template = await dbContext.Templates.FindAsync(templateId);
            template.Name = "Updated Template Name";
            template.Description = "Updated description";
            template.BookingWindowDays = 21;

            // Act
            var result = await service.UpdateTemplateAsync(template);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(templateId, result.Id);
            Assert.Equal("Updated Template Name", result.Name);
            Assert.Equal("Updated description", result.Description);
            Assert.Equal(21, result.BookingWindowDays);
            
            // Verify it was updated in the database
            var updatedTemplate = await dbContext.Templates.FindAsync(templateId);
            Assert.NotNull(updatedTemplate);
            Assert.Equal("Updated Template Name", updatedTemplate.Name);
        }

        [Fact]
        public async Task DeleteTemplateAsync_DeletesTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext, _loggerMock.Object);
            var templateId = 1;
            var providerId = "test-provider-id";

            // Act
            var result = await service.DeleteTemplateAsync(templateId, providerId);

            // Assert
            Assert.True(result);
            
            // Verify it was deleted from the database
            var deletedTemplate = await dbContext.Templates.FindAsync(templateId);
            Assert.Null(deletedTemplate);
        }
    }
} 