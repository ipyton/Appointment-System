using System;

namespace Appointment_System.Models
{
    public class Slot
    {
        public int Id { get; set; }

        public int DayId { get; set; }

        public TimeSpan duration;
        
        // Add missing properties
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxConcurrentAppointments { get; set; } = 1;
        
        // Navigation property
        public virtual Appointment? Appointment { get; set; }
    }
}
