using System;
using System.Collections.Generic;
using System.Linq;
using Appointment_System.Models;
using Appointment_System.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Appointment_System.Services.Mappers
{
    public static class ServiceMapper
    {
        public static Service MapToService(ServiceCreationDto dto, string providerId)
        {
            return new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ProviderId = providerId,
                IsActive = true,
                enabled = true,
                CreatedAt = DateTime.UtcNow,
                allowMultipleBookings = false
            };
        }

        public static List<Arrangement> MapToArrangements(ServiceCreationDto dto, int serviceId)
        {
            var arrangements = new List<Arrangement>();
            
            if (dto.ScheduleData != null && dto.ScheduleData.Any())
            {
                foreach (var item in dto.ScheduleData)
                {
                    if (DateOnly.TryParse(item.StartDate, out DateOnly startDate))
                    {
                        arrangements.Add(new Arrangement
                        {
                            ServiceId = serviceId,
                            Index = item.Order,
                            StartDate = startDate,
                            TemplateId = item.TemplateId
                        });
                    }
                }
            }
            
            return arrangements;
        }
        
        /// <summary>
        /// Generates slots for a service based on its arrangements, templates, days, and segments
        /// </summary>
        /// <param name="serviceId">The ID of the service</param>
        /// <param name="duration">Duration of the service in minutes</param>
        /// <param name="dbContext">Database context</param>
        /// <returns>List of slots</returns>
        public static async Task<List<Slot>> GenerateSlotsFromService(
            int serviceId, 
            int duration, 
            DbContext dbContext)
        {
            var slots = new List<Slot>();
            
            // Get all arrangements for this service
            var arrangements = await dbContext.Set<Arrangement>()
                .Where(a => a.ServiceId == serviceId)
                .ToListAsync();
                
            foreach (var arrangement in arrangements)
            {
                // Get the template for this arrangement
                var template = await dbContext.Set<Template>()
                    .Include(t => t.Days)
                    .ThenInclude(d => d.Segments)
                    .FirstOrDefaultAsync(t => t.Id == arrangement.TemplateId);
                
                if (template == null) continue;
                
                // Process each day in the template
                foreach (var day in template.Days.Where(d => d.IsAvailable))
                {
                    // Calculate actual dates for this day based on arrangement start date
                    var currentDate = arrangement.StartDate;
                    int daysToAdd = (day.Index - (int)currentDate.DayOfWeek + 7) % 7;
                    
                    // If daysToAdd is 0 and it's not the same day as start date, add 7 days
                    if (daysToAdd == 0 && (int)currentDate.DayOfWeek != day.Index)
                    {
                        daysToAdd = 7;
                    }
                    
                    var targetDate = currentDate.AddDays(daysToAdd);
                    
                    // Process each segment for this day
                    foreach (var segment in day.Segments)
                    {
                        var currentTime = segment.StartTime;
                        
                        // Create slots with the specified duration
                        while (currentTime.AddMinutes(duration) <= segment.EndTime)
                        {
                            slots.Add(new Slot
                            {
                                ServiceId = serviceId,
                                Date = targetDate,
                                StartTime = currentTime,
                                EndTime = currentTime.AddMinutes(duration),
                                MaxConcurrentAppointments = 1,
                                CurrentAppointmentCount = 0,
                                IsAvailable = true
                            });
                            
                            currentTime = currentTime.AddMinutes(duration);
                        }
                    }
                }
            }
            
            return slots;
        }
    }
} 