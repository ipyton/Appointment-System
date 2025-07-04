using HotChocolate.Types;
using Appointment_System.Models;

namespace Appointment_System.GraphQL.Types
{
    public class ServiceType : ObjectType<Service>
    {
        protected override void Configure(IObjectTypeDescriptor<Service> descriptor)
        {
            descriptor.Description("Represents a service that can be booked");

            descriptor.Field(s => s.Id).Description("The unique identifier for the service");
            descriptor.Field(s => s.Name).Description("The name of the service");
            descriptor.Field(s => s.Description).Description("A description of the service");
            descriptor.Field(s => s.Price).Description("The price of the service");
            descriptor.Field(s => s.ProviderId).Description("The service provider who offers this service");
            descriptor.Field(s => s.IsActive).Description("Whether the service is currently active");
            descriptor.Field(s => s.enabled).Description("Whether the service is enabled");
            descriptor.Field(s => s.CreatedAt).Description("When the service was created");
            descriptor.Field(s => s.UpdatedAt).Description("When the service was last updated");
            descriptor.Field(s => s.allowMultipleBookings).Description("Whether multiple bookings are allowed for this service");
            
            // We'll leave out the Arrangements collection for now to keep it simple
            descriptor.Ignore(s => s.Arrangements);
        }
    }
} 