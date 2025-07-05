using System;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Appointment_System.GraphQL.Extensions
{
    public static class ObjectFieldDescriptorExtensions
    {
        public static IObjectFieldDescriptor UseDbContext(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor;
        }

        public static IObjectFieldDescriptor UseDbContext(
            this IObjectFieldDescriptor descriptor,
            Type dbContextType)
        {
            return descriptor;
        }
    }
} 