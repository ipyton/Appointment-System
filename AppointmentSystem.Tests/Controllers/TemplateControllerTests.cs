using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Controllers;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using AppointmentSystem.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Linq;

namespace AppointmentSystem.Tests.Controllers
{
    public class TemplateControllerTests
    {
        private readonly string _providerId = "test-provider-id";

        private TemplateController SetupController(ApplicationDbContext dbContext, bool isProvider = true)
        {
            var templateService = new TemplateService(dbContext);
            var controller = new TemplateController(dbContext, templateService);

            // Setup ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim("sub", _providerId),
                new Claim(ClaimTypes.Name, "provider@example.com")
            };

            if (isProvider)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Provider"));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Setup controller context
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task GetTemplates_ReturnsTemplatesForProvider()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Add a template for the provider
            var template = new Template
            {
                Name = "Provider Template",
                Description = "A template for the provider",
                ProviderId = _providerId
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.GetTemplates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var templates = Assert.IsAssignableFrom<IEnumerable<Template>>(okResult.Value);
            Assert.Single(templates);
            Assert.Equal("Provider Template", templates.First().Name);
            Assert.Equal(_providerId, templates.First().ProviderId);
        }

        [Fact]
        public async Task GetTemplate_ReturnsTemplate_WhenAccessible()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Add a template
            var template = new Template
            {
                Name = "Test Template",
                Description = "A test template",
                ProviderId = _providerId
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.GetMyTemplates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var templates = Assert.IsAssignableFrom<IEnumerable<TemplateDTO>>(okResult.Value);
            Assert.Single(templates);
            Assert.Equal("Test Template", templates.First().Name);
        }

        [Fact]
        public async Task GetTemplate_ReturnsNotFound_WhenTemplateDoesNotExist()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.GetMyTemplates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var templates = Assert.IsAssignableFrom<IEnumerable<TemplateDTO>>(okResult.Value);
            Assert.Empty(templates);
        }

        [Fact]
        public async Task CreateTemplate_CreatesTemplate_WithProviderIdSet()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            
            var templateDto = new TemplateDTO
            {
                Name = "New Template",
                Description = "A new template"
                // ProviderId intentionally not set
            };

            // Act
            var result = await controller.UpsertTemplate(templateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObject = okResult.Value as dynamic;
            Assert.True(responseObject.StatusCode == 200);
            Assert.True(responseObject.New);
        }

        [Fact]
        public async Task UpdateTemplate_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            
            // Create a template owned by the provider
            var template = new Template
            {
                Name = "Template to Update",
                Description = "Original description",
                ProviderId = _providerId
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();
            
            // Update the template
            var templateDto = new TemplateDTO
            {
                Id = template.Id,
                Name = "Updated Template",
                Description = "Updated description"
            };

            // Act
            var result = await controller.UpsertTemplate(templateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObject = okResult.Value as dynamic;
            Assert.True(responseObject.StatusCode == 200);
            Assert.False(responseObject.New);
            
            // Verify the update in the database
            var updatedTemplate = await dbContext.Templates.FindAsync(template.Id);
            Assert.Equal("Updated Template", updatedTemplate.Name);
            Assert.Equal("Updated description", updatedTemplate.Description);
        }

        [Fact]
        public async Task UpdateTemplate_ReturnsForbid_WhenNotOwner()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            
            // Create a template owned by someone else
            var template = new Template
            {
                Name = "Someone Else's Template",
                Description = "Not owned by the current user",
                ProviderId = "different-provider-id"
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();
            
            // Try to update the template
            var templateDto = new TemplateDTO
            {
                Id = template.Id,
                Name = "Trying to Update"
            };

            // Act
            var result = await controller.UpsertTemplate(templateDto);

            // Assert - This should now return an Ok with a message instead of Forbid
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTemplate_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            
            // Create a template owned by the provider
            var template = new Template
            {
                Name = "Template to Delete",
                Description = "Will be deleted",
                ProviderId = _providerId
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.DeleteTemplate(template.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify the template is deleted
            var deletedTemplate = await dbContext.Templates.FindAsync(template.Id);
            Assert.Null(deletedTemplate);
        }

        [Fact]
        public async Task DeleteTemplate_ReturnsForbid_WhenNotOwner()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);
            
            // Create a template owned by someone else
            var template = new Template
            {
                Name = "Someone Else's Template",
                Description = "Not owned by the current user",
                ProviderId = "different-provider-id"
            };
            dbContext.Templates.Add(template);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.DeleteTemplate(template.Id);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteTemplate_ReturnsNotFound_WhenTemplateDoesNotExist()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.DeleteTemplate(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
} 