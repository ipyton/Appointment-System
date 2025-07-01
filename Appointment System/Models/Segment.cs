using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Segment
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }

        public int DayId { get; set; }
        
        public TimeSpan DurationForSingleSlot { get; set; }
        
        public TimeOnly StartTime { get; set; }
        
        public TimeOnly EndTime { get; set; }
                                

    }
} 