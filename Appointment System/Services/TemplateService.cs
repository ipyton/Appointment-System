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
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }



        /// <summary>
        /// Creates or updates a template based on whether it exists
        /// </summary>
        public async Task<Template> UpsertTemplateAsync(Template template)
        {
            // Store the days and segments before we detach them
            var days = template.Days?.ToList();
            
            if (template.Id > 0 && await TemplateExistsAsync(template.Id))
            {
                // Update existing template
                template.UpdatedAt = DateTime.UtcNow;
                
                // Detach days and segments to handle them separately
                template.Days = null;
                
                // Update just the template
                _context.Entry(template).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                // Now handle days and segments if they exist
                if (days != null && days.Any())
                {
                    // Get existing days to determine which to update vs. add
                    var existingDays = await _context.Days
                        .Where(d => d.TemplateId == template.Id)
                        .Include(d => d.Segments)
                        .ToListAsync();
                    
                    // Remove days not in the updated list
                    foreach (var existingDay in existingDays)
                    {
                        if (!days.Any(d => d.Index == existingDay.Index))
                        {
                            _context.Days.Remove(existingDay);
                        }
                    }
                    
                    // Update or add days
                    foreach (var day in days)
                    {
                        // Store segments before detaching them
                        var segments = day.Segments?.ToList();
                        day.Segments = null;
                        
                        // Set template ID
                        day.TemplateId = template.Id;
                        
                        // Find if this day already exists
                        var existingDay = existingDays.FirstOrDefault(d => d.Index == day.Index);
                        if (existingDay != null)
                        {
                            // Update existing day
                            existingDay.IsAvailable = day.IsAvailable;
                            _context.Entry(existingDay).State = EntityState.Modified;
                        }
                        else
                        {
                            // Add new day
                            _context.Days.Add(day);
                        }
                        
                        // Save to get day IDs
                        await _context.SaveChangesAsync();
                        
                        // Now handle segments if they exist
                        if (segments != null && segments.Any())
                        {
                            int dayId = existingDay?.Id ?? day.Id;
                            
                            // Get existing segments
                            var existingSegments = await _context.Segments
                                .Where(s => s.DayId == dayId)
                                .ToListAsync();
                            
                            // Remove segments not in the updated list
                            foreach (var existingSegment in existingSegments)
                            {
                                _context.Segments.Remove(existingSegment);
                            }
                            
                            // Add new segments
                            foreach (var segment in segments)
                            {
                                segment.DayId = dayId;
                                _context.Segments.Add(segment);
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                // Create new template
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                
                // Detach days and segments to handle them separately
                var tempDays = days;
                template.Days = null;
                
                // Save template first
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
                
                // Now handle days and segments if they exist
                if (tempDays != null && tempDays.Any())
                {
                    foreach (var day in tempDays)
                    {
                        // Store segments before detaching them
                        var segments = day.Segments?.ToList();
                        day.Segments = null;
                        
                        // Set template ID
                        day.TemplateId = template.Id;
                        
                        // Add the day
                        _context.Days.Add(day);
                        await _context.SaveChangesAsync();
                        
                        // Now handle segments if they exist
                        if (segments != null && segments.Any())
                        {
                            foreach (var segment in segments)
                            {
                                segment.DayId = day.Id;
                                segment.TemplateId = template.Id;
                                _context.Segments.Add(segment);
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            
            // Reload the complete template with all related data
            return await GetTemplateWithDetailsAsync(template.Id);
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