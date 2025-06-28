using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Appointment_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 添加用户额外属性
        public string FullName { get; set; }
        
        public string Address { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public bool IsServiceProvider { get; set; } = false;
        
        public string ProfilePictureUrl { get; set; }
        
        // For service providers
        public string BusinessName { get; set; }
        
        public string BusinessDescription { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Service> Services { get; set; }
        
        public virtual ICollection<Appointment> UserAppointments { get; set; }
        
        public virtual ICollection<Appointment> ProviderAppointments { get; set; }
    }
} 