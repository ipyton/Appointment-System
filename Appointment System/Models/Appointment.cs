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


        [Required]
        [ForeignKey("ProviderId")]
        public int ProviderId { get; set; }


        [Required]
        public int SlotId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Add missing properties for time management
        public DateTime AppointmentDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        // Payment related properties
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaymentAmount { get; set; }
        
        [StringLength(3)]
        public string PaymentCurrency { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(500)]
        public string SpecialRequests { get; set; }
        
        // Contact information
        [StringLength(100)]
        [EmailAddress]
        public string ContactEmail { get; set; }
        
        [StringLength(20)]
        public string ContactPhone { get; set; }
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