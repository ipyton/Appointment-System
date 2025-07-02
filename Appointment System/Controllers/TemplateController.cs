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

namespace Appointment_System.Controllers
{
    [Route("api/[controller]")]
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
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            var templates = await _templateService.GetTemplatesForProviderAsync(providerId);
            return Ok(templates);
        }

        // GET: api/Template/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Template>> GetTemplate(int id)
        {
            var template = await _templateService.GetTemplateWithDetailsAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            // If not provider, check if the template is accessible
            if (!User.IsInRole("Provider") || User.FindFirst("sub")?.Value != template.ProviderId)
            {
                // Check if the template is associated with a public service
                var isPublicService = await _context.Services
                    .AnyAsync(s => s.ProviderId == template.ProviderId && s.IsPublic);
                
                if (!isPublicService)
                {
                    return Forbid();
                }
            }

            return template;
        }

        // POST: api/Template
        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<Template>> CreateTemplate(Template template)
        {
            // Ensure the provider ID is set to the current user
            if (User.Identity.IsAuthenticated)
            {
                template.ProviderId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            }

            var createdTemplate = await _templateService.CreateTemplateAsync(template);
            return CreatedAtAction(nameof(GetTemplate), new { id = createdTemplate.Id }, createdTemplate);
        }

        // PUT: api/Template/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateTemplate(int id, Template template)
        {
            if (id != template.Id)
            {
                return BadRequest();
            }

            // Verify ownership
            var existingTemplate = await _context.Templates.FindAsync(id);
            if (existingTemplate == null)
            {
                return NotFound();
            }

            // Check if the current user owns this template
            var currentUserId = User.FindFirst("sub")?.Value ?? User.Identity.Name;
            if (existingTemplate.ProviderId != currentUserId)
            {
                return Forbid();
            }

            // Update the template
            var success = await _templateService.UpdateTemplateAsync(template);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Template/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Provider")]
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
