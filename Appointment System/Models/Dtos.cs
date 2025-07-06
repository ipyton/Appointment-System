using System;
using System.ComponentModel.DataAnnotations;
using Appointment_System.Models;

namespace Appointment_System.Models
{
    public class ServiceDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(5, 480)]
        public int DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ScheduleDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        public int? RepeatWeeks { get; set; }

        [Required]
        [Range(5, 120)]
        public int SlotDurationMinutes { get; set; } = 30;
    }

    public class WeeklyAvailabilityDto
    {
        [Required]
        public int ServiceScheduleId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }
    }

    public class TimeSlotDto
    {
        [Required]
        public int WeeklyAvailabilityId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxConcurrentAppointments { get; set; } = 1;
    }

    public class UpdateStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }
    }

    public class SendMessageDto
    {
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }
}
