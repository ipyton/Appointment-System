using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models.DTOs
{
    public class ServiceCreationDto
    {
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public int Duration { get; set; }
        
        public string EventImage { get; set; }
        
        public string StartDate { get; set; }
        
        public List<ArrangementDto> ScheduleData { get; set; } = new List<ArrangementDto>();
    }
} 