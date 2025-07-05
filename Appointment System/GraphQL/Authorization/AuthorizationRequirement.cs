using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Appointment_System.GraphQL.Authorization
{
    public class AuthorizationRequirement : IAuthorizationRequirement
    {
        public List<string> Roles { get; }

        public AuthorizationRequirement(string[] roles = null)
        {
            Roles = new List<string>(roles ?? Array.Empty<string>());
        }
    }
} 