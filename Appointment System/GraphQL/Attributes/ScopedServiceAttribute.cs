using System;

namespace Appointment_System.GraphQL.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ScopedServiceAttribute : Attribute
    {
        // This is just a marker attribute to make the code compile
        // It doesn't need any implementation
    }
} 