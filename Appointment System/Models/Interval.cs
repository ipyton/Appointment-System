using System;

namespace Appointment_System.Models
{
    public class Interval
    {
        public int Id { get; set; }

        public int DayId { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan DurationForASlot { get; set; }
    }
}
