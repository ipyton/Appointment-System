using System;
using System.Collections.Generic;
using System.Linq;
using Appointment_System.Models;
using Appointment_System.Models.DTOs;

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
    }
} 