using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        
        [Required]
        [RegularExpression("^(ServiceProvider|User)$", ErrorMessage = "Role must be either 'ServiceProvider' or 'User'")]
        public string Role { get; set; } = "User";
    }
}
