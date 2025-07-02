using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public bool enabled { get; set; } = true;

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


        [Required]
        public string ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public ApplicationUser Provider { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        
        public bool allowMultipleBookings { get; set; } = false;
        
        //public virtual ICollection<Segment> Segments { get; set; }
        
        public virtual ICollection<Arrangement> Arrangements { get; set; }
        
        /// <summary>
        /// Maximum number of days in advance that appointments can be booked
        /// </summary>
        //public int BookingWindowDays { get; set; } = 30;

        /// <summary>
        /// Minimum notice period required for booking (in hours)
        /// </summary>
        //public int MinimumNoticeHours { get; set; } = 24;
    }
} 