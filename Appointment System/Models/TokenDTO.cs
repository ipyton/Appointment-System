using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models
{
    public class TokenDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
} 