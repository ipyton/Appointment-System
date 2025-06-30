using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class TokenRecord
    {
        public ApplicationUser ApplicationUser { get; set; }
        
        [Key]
        [Required]
        public string AccessToken { get; set; } = string.Empty;
        
        public DateTimeOffset ExpiresOn { get; set; }
        
        [Required]
        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
    }
}
