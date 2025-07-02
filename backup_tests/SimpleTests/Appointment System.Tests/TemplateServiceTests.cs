using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Appointment_System.Tests
{
    public class TemplateServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public TemplateServiceTests()
        {
            // Set up in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            // Seed the database
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                SeedDatabase(context);
            }
        }

        [Fact]
        public async Task GetTemplatesForProviderAsync_ReturnsProviderTemplates()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new TemplateService(context);
            var providerId = "provider1";

            // Act
            var result = await service.GetTemplatesForProviderAsync(providerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // Provider1 has 2 templates in our seed data
            Assert.All(result, t => Assert.Equal(providerId, t.ProviderId));
        }

        [Fact]
        public async Task GetTemplateWithDetailsAsync_WithValidId_ReturnsTemplateWithDetails()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new TemplateService(context);
            var templateId = 1;

            // Act
            var result = await service.GetTemplateWithDetailsAsync(templateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(templateId, result.Id);
            Assert.Equal("Weekly Schedule", result.Name);
            Assert.NotNull(result.Days);
            Assert.Equal(2, result.Days.Count); // Template 1 has 2 days in our seed data
        }

        [Fact]
        public async Task CreateTemplateAsync_CreatesNewTemplate()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new TemplateService(context);
            var newTemplate = new Template
            {
                Name = "New Test Template",
                Description = "A new template for testing",
                ProviderId = "provider1",
                Type = false,
                IsActive = true
            };

            // Act
            var result = await service.CreateTemplateAsync(newTemplate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Test Template", result.Name);
            Assert.Equal("provider1", result.ProviderId);
            Assert.NotEqual(default, result.Id); // ID should be assigned
            Assert.NotNull(result.Days); // Default days should be created
        }

        [Fact]
        public async Task UpdateTemplateAsync_WithValidTemplate_UpdatesTemplate()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new TemplateService(context);
            
            // First get the existing template
            var existingTemplate = await context.Templates.FindAsync(1);
            existingTemplate.Name = "Updated Template Name";
            existingTemplate.Description = "Updated description";

            // Act
            var result = await service.UpdateTemplateAsync(existingTemplate);

            // Assert
            Assert.True(result);
            
            // Verify the update in the database
            var updatedTemplate = await context.Templates.FindAsync(1);
            Assert.Equal("Updated Template Name", updatedTemplate.Name);
            Assert.Equal("Updated description", updatedTemplate.Description);
            Assert.NotNull(updatedTemplate.UpdatedAt);
        }

        [Fact]
        public async Task DeleteTemplateAsync_WithValidId_DeletesTemplate()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions);
            var service = new TemplateService(context);
            var templateId = 2;

            // Act
            var result = await service.DeleteTemplateAsync(templateId);

            // Assert
            Assert.True(result);
            
            // Verify the template is deleted
            var deletedTemplate = await context.Templates.FindAsync(templateId);
            Assert.Null(deletedTemplate);
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
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Template
                {
                    Id = 2,
                    Name = "Special Event Schedule",
                    Description = "Schedule for special events",
                    ProviderId = provider1.Id,
                    Provider = provider1,
                    Type = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Template
                {
                    Id = 3,
                    Name = "Other Provider Schedule",
                    Description = "Schedule for another provider",
                    ProviderId = provider2.Id,
                    Provider = provider2,
                    Type = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
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
                },
                new Day
                {
                    Id = 2,
                    TemplateId = 1,
                    DayOfWeek = DayOfWeek.Wednesday,
                    IsActive = true
                },
                new Day
                {
                    Id = 3,
                    TemplateId = 2,
                    DayOfWeek = DayOfWeek.Saturday,
                    IsActive = true
                },
                new Day
                {
                    Id = 4,
                    TemplateId = 3,
                    DayOfWeek = DayOfWeek.Friday,
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
                },
                new Segment
                {
                    Id = 2,
                    DayId = 1,
                    StartTime = TimeSpan.FromHours(13),
                    EndTime = TimeSpan.FromHours(17)
                },
                new Segment
                {
                    Id = 3,
                    DayId = 2,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(15)
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
            context.SaveChanges();
        }
    }
} 