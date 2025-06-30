using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class ServiceSchedule
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ServiceId { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
        
        // When this schedule becomes active
        [Required]
        public DateTime StartDate { get; set; }
        
        // How many weeks this should repeat (null means forever)
        public int? RepeatWeeks { get; set; }
        
        // Length of each appointment slot in minutes
        [Required]
        public int SlotDurationMinutes { get; set; } = 30;
        
        // Whether this schedule is currently active
        public bool IsActive { get; set; } = true;
        
        // Standard timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 