using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;

namespace Appointment_System.Services
{
    public class TemplateService
    {
        private readonly ApplicationDbContext _context;

        public TemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all templates for a specific provider
        /// </summary>
        public async Task<IEnumerable<Template>> GetTemplatesForProviderAsync(string providerId)
        {
            return await _context.Templates
                .Where(t => t.ProviderId == providerId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a template by ID with all related data
        /// </summary>
        public async Task<Template> GetTemplateWithDetailsAsync(int templateId)
        {
            return await _context.Templates
                .Include(t => t.Days)
                    .ThenInclude(d => d.Segments)
                        .ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }

        /// <summary>
        /// Creates a new template
        /// </summary>
        public async Task<Template> CreateTemplateAsync(Template template)
        {
            // Set creation date
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            
            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            
            return template;
        }

        /// <summary>
        /// Updates an existing template
        /// </summary>
        public async Task<bool> UpdateTemplateAsync(Template template)
        {
            // Set update date
            template.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(template).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TemplateExistsAsync(template.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Creates or updates a template based on whether it exists
        /// </summary>
        public async Task<Template> UpsertTemplateAsync(Template template)
        {
            if (template.Id > 0 && await TemplateExistsAsync(template.Id))
            {
                // Update existing template
                template.UpdatedAt = DateTime.UtcNow;
                _context.Entry(template).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create new template
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
            }
            
            return template;
        }

        /// <summary>
        /// Deletes a template and all related data
        /// </summary>
        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            // Get the template with all related data
            var template = await _context.Templates
                .Include(t => t.Days)
                    .ThenInclude(d => d.Segments)
                        .ThenInclude(s => s.Slots)
                .FirstOrDefaultAsync(t => t.Id == templateId);
                
            if (template == null)
            {
                return false;
            }
            
            // Remove the template and all related data
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Checks if a template exists
        /// </summary>
        private async Task<bool> TemplateExistsAsync(int templateId)
        {
            return await _context.Templates.AnyAsync(t => t.Id == templateId);
        }
    }
} 