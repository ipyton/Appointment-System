using Appointment_System.Models;
using HotChocolate.Types;

namespace Appointment_System.GraphQL.Types
{
    public class AppointmentType : ObjectType<Appointment>
    {
        protected override void Configure(IObjectTypeDescriptor<Appointment> descriptor)
        {
            descriptor.Description("Represents an appointment in the system");

            descriptor.Field(a => a.Id).Description("The unique identifier for the appointment");
            descriptor.Field(a => a.UserId).Description("The user who made the appointment");
            descriptor.Field(a => a.ServiceId).Description("The service being provided");
            descriptor.Field(a => a.ProviderId).Description("The service provider");
            descriptor.Field(a => a.SlotId).Description("The time slot for the appointment");
            descriptor.Field(a => a.Slot).Description("The slot details for the appointment");
            descriptor.Field(a => a.Notes).Description("Notes about the appointment");
            descriptor.Field(a => a.Status).Description("The current status of the appointment");
            descriptor.Field(a => a.CreatedAt).Description("When the appointment was created");
            descriptor.Field(a => a.UpdatedAt).Description("When the appointment was last updated");
        }
    }
}
