using HotChocolate.Types;
using Appointment_System.Models;

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
            descriptor.Field(a => a.Notes).Description("Notes about the appointment");
            descriptor.Field(a => a.Status).Description("The current status of the appointment");
            descriptor.Field(a => a.CreatedAt).Description("When the appointment was created");
            descriptor.Field(a => a.UpdatedAt).Description("When the appointment was last updated");
            descriptor.Field(a => a.AppointmentDate).Description("The date of the appointment");
            descriptor.Field(a => a.StartTime).Description("The start time of the appointment");
            descriptor.Field(a => a.EndTime).Description("The end time of the appointment");
        }
    }
} 