using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Arrangement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ServiceId")]
        public int ServiceId { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
        
        [ForeignKey("TemplateId")]
        public int TemplateId { get; set; }
    }
}
