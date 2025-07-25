using System.ComponentModel.DataAnnotations;

namespace Appointment_System.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Full Name must be less than 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long and less than {1} characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;



        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User"; // Default role is User
    }
} 