using Microsoft.AspNetCore.Identity;

namespace Appointment_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 添加用户额外属性
        public string? FullName { get; set; }
    }
} 