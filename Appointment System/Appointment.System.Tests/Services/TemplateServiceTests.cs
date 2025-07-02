using System;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Models;
using Appointment_System.Services;
using Appointment.System.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Appointment.System.Tests.Services
{
    public class TemplateServiceTests
    {
        [Fact]
        public async Task GetTemplatesForProviderAsync_ReturnsTemplatesForProvider()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var service = new TemplateService(dbContext);
            
            // Add a test template for our provider
            var providerId = "test-provider-id";
            var template = new Template
            {
                Name = "Provider Test Template",
                Description = "A test template for a specific provider",
                ProviderId = providerId,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetTemplatesForProviderAsync(providerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Provider Test Template", result.First().Name);
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
                DayOfWeek = DayOfWeek.Monday,
                IsAvailable = true,
                StartTimeMinutes = 9 * 60,
                EndTimeMinutes = 17 * 60
            };
            dbContext.Days.Add(day);
            await dbContext.SaveChangesAsync();
            
            // Add segments
            var segment = new Segment
            {
                DayId = day.Id,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(12, 0, 0)
            };
            dbContext.Segments.Add(segment);
            await dbContext.SaveChangesAsync();
            
            // Add slots
            var slot = new Slot
            {
                DayId = day.Id,
                SegmentId = segment.Id,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0)
            };
            dbContext.Slots.Add(slot);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetTemplateWithDetailsAsync(template.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Detailed Template", result.Name);
            Assert.NotEmpty(result.Days);
            Assert.Single(result.Days);
            
            var resultDay = result.Days.First();
            Assert.Equal(DayOfWeek.Monday, resultDay.DayOfWeek);
            Assert.NotEmpty(resultDay.Segments);
            Assert.Single(resultDay.Segments);
            
            var resultSegment = resultDay.Segments.First();
            Assert.Equal(new TimeSpan(9, 0, 0), resultSegment.StartTime);
            Assert.NotEmpty(resultSegment.Slots);
            Assert.Single(resultSegment.Slots);
            
            var resultSlot = resultSegment.Slots.First();
            Assert.Equal(new TimeSpan(9, 0, 0), resultSlot.StartTime);
        }

        [Fact]
        public async Task CreateTemplateAsync_CreatesTemplateWithDefaultDays()
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
            var result = await service.CreateTemplateAsync(template);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Template", result.Name);
            Assert.NotNull(result.Days);
            Assert.Equal(7, result.Days.Count); // 7 days of the week
            
            // Verify weekdays are available and weekends are not
            Assert.False(result.Days.First(d => d.Index == 0).IsAvailable); // Sunday
            Assert.True(result.Days.First(d => d.Index == 1).IsAvailable);  // Monday
            Assert.True(result.Days.First(d => d.Index == 2).IsAvailable);  // Tuesday
            Assert.True(result.Days.First(d => d.Index == 3).IsAvailable);  // Wednesday
            Assert.True(result.Days.First(d => d.Index == 4).IsAvailable);  // Thursday
            Assert.True(result.Days.First(d => d.Index == 5).IsAvailable);  // Friday
            Assert.False(result.Days.First(d => d.Index == 6).IsAvailable); // Saturday
        }

        [Fact]
        public async Task UpdateTemplateAsync_UpdatesExistingTemplate()
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
            var result = await service.UpdateTemplateAsync(template);

            // Assert
            Assert.True(result);
            
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
            
            var day = new Day
            {
                TemplateId = template.Id,
                DayOfWeek = DayOfWeek.Monday
            };
            dbContext.Days.Add(day);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.DeleteTemplateAsync(template.Id);

            // Assert
            Assert.True(result);
            
            // Verify the template is deleted
            var deletedTemplate = await dbContext.Templates.FindAsync(template.Id);
            Assert.Null(deletedTemplate);
            
            // Verify related day is deleted
            var deletedDay = await dbContext.Days.FirstOrDefaultAsync(d => d.TemplateId == template.Id);
            Assert.Null(deletedDay);
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