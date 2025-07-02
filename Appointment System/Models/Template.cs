using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    /// <summary>
    /// Represents a template for scheduling appointments
    /// A template defines the available days and time slots for a service provider
    /// </summary>
    public class Template
    {
        [Key]
        public int Id { get; set; } = 0;

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Template type:
        /// - false: Slot-based model (predefined time slots)
        /// - true: Free-selection model (flexible time selection)
        /// </summary>
        public bool Type { get; set; }

        /// <summary>
        /// The ID of the service provider who owns this template
        /// </summary>
        [Required]
        public string ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public ApplicationUser Provider { get; set; }

        /// <summary>
        /// Collection of days configured in this template
        /// </summary>
        public ICollection<Day> Days { get; set; }

        /// <summary>
        /// Indicates if the template is active and can be used for scheduling
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the template was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the template was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }


    }
}
 