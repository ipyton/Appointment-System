using System;
using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models.DTOs
{
    public class ArrangementDto
    {
        public string Id { get; set; }
        
        [Required]
        public int TemplateId { get; set; }
        
        public string TemplateName { get; set; }
        
        [Required]
        public string StartDate { get; set; }
        
        [Required]
        public int Order { get; set; }
    }
} 