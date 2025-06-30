using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class WeeklyAvailability
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ServiceScheduleId { get; set; }
        
        [ForeignKey("ServiceScheduleId")]
        public ServiceSchedule ServiceSchedule { get; set; }
        
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        // Navigation property for time slots on this day
        public virtual ICollection<TimeSlot> TimeSlots { get; set; }
        
        // Standard timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int WeeklyAvailabilityId { get; set; }
        
        [ForeignKey("WeeklyAvailabilityId")]
        public WeeklyAvailability WeeklyAvailability { get; set; }
        
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan EndTime { get; set; }
        
        // Maximum number of concurrent appointments allowed in this slot
        public int MaxConcurrentAppointments { get; set; } = 1;
        
        // Standard timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 