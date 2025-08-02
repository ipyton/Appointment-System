using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models
{
    public class MessageDto
    {
        [Required]
        public string ReceiverId { get; set; }
        
        [Required]
        public string Content { get; set; }
    }
} 