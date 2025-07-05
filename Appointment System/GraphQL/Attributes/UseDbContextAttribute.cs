using System;
using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Appointment_System.GraphQL.Extensions;

namespace Appointment_System.GraphQL.Attributes
{
    public class UseDbContextAttribute : ObjectFieldDescriptorAttribute
    {
        private readonly Type _dbContextType;

        public UseDbContextAttribute(Type dbContextType = null)
        {
            _dbContextType = dbContextType;
        }

        protected override void OnConfigure(
            IDescriptorContext context,
            IObjectFieldDescriptor descriptor,
            MemberInfo member)
        {
            if (_dbContextType != null)
            {
                descriptor.UseDbContext(_dbContextType);
            }
            else
            {
                descriptor.UseDbContext();
            }
        }
    }
} 