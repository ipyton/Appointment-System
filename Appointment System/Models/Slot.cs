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


        [Required]
        public int ServiceId { get; set;}


        [Required]
        public DateOnly Date { get; set;}
        

        /// <summary>
        /// Start time of the slot
        /// </summary>
        [Required]
        public TimeOnly StartTime { get; set; }
        
        /// <summary>
        /// End time of the slot
        /// </summary>
        [Required]
        public TimeOnly EndTime { get; set; }
        
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
        
    }
}
