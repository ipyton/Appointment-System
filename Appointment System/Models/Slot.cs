using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    /// <summary>
    /// Represents a specific time slot for an appointment
    /// </summary>
    public class Slot
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the day this slot belongs to
        /// </summary>
        public int DayId { get; set; }
        
        /// <summary>
        /// The ID of the segment this slot belongs to
        /// </summary>
        public int SegmentId { get; set; }
        
        [ForeignKey("SegmentId")]
        public Segment Segment { get; set; }

        /// <summary>
        /// Duration of the slot
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Start time of the slot
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// End time of the slot
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Maximum number of concurrent appointments allowed in this slot
        /// </summary>
        public int MaxConcurrentAppointments { get; set; } = 1;
        
        /// <summary>
        /// Current number of appointments booked for this slot
        /// </summary>
        public int CurrentAppointmentCount { get; set; } = 0;
        
        /// <summary>
        /// Whether this slot is available for booking
        /// </summary>
        public bool IsAvailable { get; set; } = true;
        
        /// <summary>
        /// Navigation property to the appointment booked for this slot
        /// </summary>
        public virtual Appointment? Appointment { get; set; }
    }
}
