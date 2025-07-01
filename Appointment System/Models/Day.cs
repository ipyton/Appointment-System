using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Day
    {
        public int Id { get; set; }

        public int Index { get; set; }

        public int TemplateId { get; set; }
        
        [ForeignKey("TemplateId")]
        public Template Template { get; set; }
    }
}
