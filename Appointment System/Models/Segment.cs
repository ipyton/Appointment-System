using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    /// <summary>
    /// Represents a time segment within a day for scheduling appointments
    /// </summary>
    public class Segment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the template this segment belongs to
        /// </summary>
        [ForeignKey("TemplateId")]
        public int TemplateId { get; set; }

        /// <summary>
        /// The ID of the day this segment belongs to
        /// </summary>
        public int DayId { get; set; }
        
        
        /// <summary>
        /// Duration for a single appointment slot within this segment
        /// </summary>
        public TimeSpan DurationForSingleSlot { get; set; }
        
        /// <summary>
        /// Start time of this segment
        /// </summary>
        public TimeOnly StartTime { get; set; }
        
        /// <summary>
        /// End time of this segment
        /// </summary>
        public TimeOnly EndTime { get; set; }
        
        /// <summary>
        /// Maximum number of concurrent appointments allowed in this segment
        /// </summary>
        public int MaxConcurrentAppointments { get; set; } = 1;
        
        /// <summary>
        /// Whether this segment is available for booking
        /// </summary>
        public bool IsAvailable { get; set; } = true;
        
    }
} 