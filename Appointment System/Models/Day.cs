using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    /// <summary>
    /// Represents a day in a template schedule
    /// </summary>
    public class Day
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Day index (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
        /// </summary>
        [Range(0, 6)]
        public int Index { get; set; }

        /// <summary>
        /// The ID of the template this day belongs to
        /// </summary>
        public int TemplateId { get; set; }
        
        /// <summary>
        /// Whether this day is available for appointments
        /// </summary>
        public bool IsAvailable { get; set; } = true;


        /// <summary>
        /// Collection of time segments for this day
        /// </summary>
        public ICollection<Segment> Segments { get; set; }
    }
}
