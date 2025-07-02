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
        /// Creates a new template with default days
        /// </summary>
        public async Task<Template> CreateTemplateAsync(Template template)
        {
            // Set creation date
            template.CreatedAt = DateTime.UtcNow;
            
            // Add the template to the database
            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            
            // Create default days if not provided
            if (template.Days == null || !template.Days.Any())
            {
                template.Days = CreateDefaultDays(template.Id);
                await _context.SaveChangesAsync();
            }
            
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

        /// <summary>
        /// Creates default days for a template
        /// </summary>
        private ICollection<Day> CreateDefaultDays(int templateId)
        {
            var days = new List<Day>();
            
            // Create 7 days (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
            for (int i = 0; i < 7; i++)
            {
                // By default, weekends are not available
                bool isAvailable = i != 0 && i != 6;
                
                var day = new Day
                {
                    TemplateId = templateId,
                    Index = i,
                    IsAvailable = isAvailable,
                    StartTimeMinutes = 9 * 60, // 9:00 AM
                    EndTimeMinutes = 17 * 60   // 5:00 PM
                };
                
                days.Add(day);
                _context.Days.Add(day);
            }
            
            return days;
        }
    }
} 