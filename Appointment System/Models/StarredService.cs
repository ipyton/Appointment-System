using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class StarredService
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public int ServiceId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
} 