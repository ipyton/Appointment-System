using System;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using AppointmentSystem.Tests.Helpers;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Tests.Services
{
    public class TemplateServiceTests
    {
        [Fact]
        public async Task GetTemplateWithDetailsAsync_WithValidId_ReturnsTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);

            // Act
            var result = await service.GetTemplateWithDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Template", result.Name);
        }

        [Fact]
        public async Task GetTemplateWithDetailsAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);

            // Act
            var result = await service.GetTemplateWithDetailsAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTemplatesForProviderAsync_ReturnsTemplatesForProvider()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            var providerId = "test-provider-id";

            // Act
            var result = await service.GetTemplatesForProviderAsync(providerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Template", result.First().Name);
            Assert.Equal(providerId, result.First().ProviderId);
        }

        [Fact]
        public async Task GetTemplateWithDetailsAsync_ReturnsTemplateWithAllRelatedData()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            
            // Create a new template with days, segments, and slots
            var template = new Template
            {
                Name = "Detailed Template",
                Description = "A template with all related data",
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();
            
            // Add days
            var day = new Day
            {
                TemplateId = template.Id,
                Index = 1, // Monday
                IsAvailable = true
            };
            dbContext.Days.Add(day);
            await dbContext.SaveChangesAsync();
            
            // Add segments
            var segment = new Segment
            {
                DayId = day.Id,
                TemplateId = template.Id,
                StartTime = TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                EndTime = TimeOnly.FromTimeSpan(new TimeSpan(12, 0, 0))
            };
            dbContext.Segments.Add(segment);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetTemplateWithDetailsAsync(template.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Detailed Template", result.Name);
            Assert.NotEmpty(result.Days);
            Assert.Single(result.Days);
            
            var resultDay = result.Days.First();
            Assert.Equal(1, resultDay.Index); // Monday
            Assert.NotEmpty(resultDay.Segments);
            Assert.Single(resultDay.Segments);
            
            var resultSegment = resultDay.Segments.First();
            Assert.Equal(TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)), resultSegment.StartTime);
        }

        [Fact]
        public async Task UpsertTemplateAsync_CreatesNewTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            
            var template = new Template
            {
                Name = "New Template",
                Description = "A new template with default days"
            };

            // Act
            var result = await service.UpsertTemplateAsync(template);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Template", result.Name);
            Assert.NotNull(result.Days);
        }

        [Fact]
        public async Task UpsertTemplateAsync_UpdatesExistingTemplate()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            
            // Create a template to update
            var template = new Template
            {
                Name = "Template to Update",
                Description = "Original description"
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();
            
            // Update the template
            template.Name = "Updated Template";
            template.Description = "Updated description";

            // Act
            var result = await service.UpsertTemplateAsync(template);

            // Assert
            Assert.NotNull(result);
            
            // Verify the update in the database
            var updatedTemplate = await dbContext.Templates.FindAsync(template.Id);
            Assert.Equal("Updated Template", updatedTemplate.Name);
            Assert.Equal("Updated description", updatedTemplate.Description);
            Assert.NotNull(updatedTemplate.UpdatedAt);
        }

        [Fact]
        public async Task DeleteTemplateAsync_DeletesTemplateAndRelatedData()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            
            // Create a template with related data
            var template = new Template
            {
                Name = "Template to Delete",
                Description = "Will be deleted"
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.DeleteTemplateAsync(template.Id);

            // Assert
            Assert.True(result);
            
            // Verify the template is deleted
            var deletedTemplate = await dbContext.Templates.FindAsync(template.Id);
            Assert.Null(deletedTemplate);
        }

        [Fact]
        public async Task DeleteTemplateAsync_ReturnsFalse_WhenTemplateDoesNotExist()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);

            // Act
            var result = await service.DeleteTemplateAsync(999);

            // Assert
            Assert.False(result);
        }
    }
} 