using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        [Required]
        public string UserId { get; set; }


        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public int ServiceId { get; set; }
         
        [Required]
        public int TemplateId {get; set;}

        [Required]
        public int SlotId { get; set; }

        [Required]
        public int DayId { get; set; }
        
        [Required]
        public int SegmentId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        public int BillId { get; set; }

        
        // Add missing properties for time management
        public DateTime AppointmentDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        // Navigation property for bills
        public virtual ICollection<Bill> Bills { get; set; }
    }

    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled,
        NoShow
    }
} 