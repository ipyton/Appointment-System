using System;
using System.Linq;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using System.Reflection;

namespace Appointment_System.GraphQL.Attributes
{
    public class AuthorizeAttribute : ObjectFieldDescriptorAttribute
    {
        private readonly string[] _roles;

        public string[] Roles { get; set; }

        public AuthorizeAttribute()
        {
            _roles = Array.Empty<string>();
            Roles = Array.Empty<string>();
        }

        public AuthorizeAttribute(string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
            Roles = _roles;
        }

        protected override void OnConfigure(
            IDescriptorContext context,
            IObjectFieldDescriptor descriptor,
            MemberInfo member)
        {
            // This is a dummy attribute that doesn't actually do anything
            // It's just to make the code compile
        }
    }
} 