using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using System.Security.Claims;

namespace Appointment_System.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TemplateService _templateService;

        public TemplateController(ApplicationDbContext context, TemplateService templateService)
        {
            _context = context;
            _templateService = templateService;
        }

        // GET: api/Template
        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            var templates = await _templateService.GetTemplatesForProviderAsync(providerId);
            return Ok(templates);
        }

        // GET: Template/byUser/{userId}
        [HttpGet("byUser/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplatesByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be empty");
            }

            var templates = await _templateService.GetTemplatesForProviderAsync(userId);
            
            if (templates == null || !templates.Any())
            {
                return NotFound($"No templates found for user with ID {userId}");
            }

            return Ok(templates);
        }

        [HttpGet("get")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<ActionResult<IEnumerable<TemplateDTO>>> GetMyTemplates()
        {
            var userId = User.FindFirst("sub")?.Value ?? 
                         User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                         User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID could not be determined from token");
            }

            // Get templates with their days and segments included
            var templates = await _context.Templates
                .Where(t => t.ProviderId == userId)
                .Include(t => t.Days)
                    .ThenInclude(d => d.Segments)
                .ToListAsync();
            
            if (templates == null || !templates.Any())
            {
                return Ok(new List<TemplateDTO>());
            }

            return Ok(templates.Select(t => t.ToTemplateDTO()));
        }


         [HttpGet("getNames")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<ActionResult<IEnumerable<TemplateDTO>>> GetMyTemplateNames()
        {
            var userId = User.FindFirst("sub")?.Value ?? 
                         User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                         User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID could not be determined from token");
            }

            // Get templates with their days and segments included
            var templates = await _context.Templates
                .Where(t => t.ProviderId == userId)
                .ToListAsync();
            
            if (templates == null || !templates.Any())
            {
                return Ok(new List<TemplateDTO>());
            }

            return Ok(templates.Select(t => t.ToTemplateDTO()));
        }

        // [HttpGet("get/{id}")]
        // [Authorize(Roles = "ServiceProvider")]
        // public async Task<ActionResult<Template>> GetTemplate(int id)
        // {
        //     var template = await _templateService.GetTemplateWithDetailsAsync(id).Include(t => t.Days).Include(t => t.Days.Select(d => d.Segments));

        //     if (template == null)
        //     {
        //         return NotFound($"Template with ID {id} not found");
        //     }

        //     // Check if the user has access to this template
        //     var currentUserId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        //     if (template.ProviderId != currentUserId)
        //     {
        //         return Forbid();
        //     }

        //     return Ok(template.ToTemplateDTO());
        // }


        // PUT: Template/upsert
        [HttpPut("upsert")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> UpsertTemplate(TemplateDTO templateDTO)
        {
            // Try multiple ways to get the user ID
            var currentUserId = User.FindFirst("sub")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.Identity?.Name;
                
            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User not authenticated");
            }
            
            // Convert DTO to Template
            var template = TemplateDTOExtensions.ToTemplate(templateDTO, currentUserId);
            Console.WriteLine(template.Id);
            // Check if template ID exists
            if (template.Id > 0)
            {
                // Check if the template exists and belongs to the current user
                var existingTemplate = await _context.Templates.FindAsync(template.Id);
                if (existingTemplate == null)
                {
                    return NotFound();
                } else {
                    // Check if the current user owns this template
                    if (existingTemplate.ProviderId != currentUserId)
                    {
                        return Forbid();
                    }
                }
            }
            
            // Set provider ID and timestamps
            template.ProviderId = currentUserId;
            
            // Use the service to upsert the template
            var updatedTemplate = await _templateService.UpsertTemplateAsync(template);
            if (template.Id == 0)
            {
                // New template was created
                return Ok(new { StatusCode = 200, New = true,Id = updatedTemplate.Id, Message = $"Template with ID {updatedTemplate.Id} has been saved successfully" });
            }
            else
            {
                // Existing template was updated
                return Ok(new { StatusCode = 200, New = false,Id = updatedTemplate.Id, Message = $"Template with ID {updatedTemplate.Id} has been saved successfully" });
            }

        }

        // DELETE: api/Template/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            // Verify ownership
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Check if the current user owns this template
            var currentUserId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            if (template.ProviderId != currentUserId)
            {
                return Forbid();
            }

            // Delete the template
            var success = await _templateService.DeleteTemplateAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
