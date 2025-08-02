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
        [ForeignKey("ServiceId")]
        public int ServiceId { get; set; }
        
        // Navigation property for Service
        public virtual Service Service { get; set; }


        [Required]
        [ForeignKey("ProviderId")]
        public int ProviderId { get; set; }


        [Required]
        [ForeignKey("SlotId")]
        public int SlotId { get; set; }
        
        // Navigation property for Slot
        public virtual Slot Slot { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(500)]
        public string SpecialRequests { get; set; }
        
        // Contact information
        [StringLength(100)]
        [EmailAddress]
        public string ContactEmail { get; set; }
        
        [StringLength(20)]
        public string ContactPhone { get; set; }

        public bool IsStarred { get; set; } = false;
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