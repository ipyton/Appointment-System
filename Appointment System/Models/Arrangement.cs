using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Arrangement
    {
        public int Id { get; set; }
        public int RepeatTimes { get; set; }
        public int RepeatInterval { get; set; } 
        public int ServiceId { get; set; }
        public int TemplateId { get; set; }
        public DateTime StartDateTime { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
        
        [ForeignKey("TemplateId")]
        public Template Template { get; set; }
    }
}
