using HotChocolate.Types;
using Appointment_System.Models;

namespace Appointment_System.GraphQL.Types
{
    public class UserType : ObjectType<ApplicationUser>
    {
        protected override void Configure(IObjectTypeDescriptor<ApplicationUser> descriptor)
        {
            descriptor.Description("Represents a user in the appointment system");

            descriptor.Field(u => u.Id).Description("The unique identifier for the user");
            descriptor.Field(u => u.FullName).Description("The full name of the user");
            descriptor.Field(u => u.Email).Description("The email address of the user");
            descriptor.Field(u => u.UserName).Description("The username of the user");
            descriptor.Field(u => u.Address).Description("The address of the user");
            descriptor.Field(u => u.DateOfBirth).Description("The date of birth of the user");
            descriptor.Field(u => u.IsServiceProvider).Description("Whether the user is a service provider");
            descriptor.Field(u => u.ProfilePictureUrl).Description("URL to the user's profile picture");
            descriptor.Field(u => u.BusinessName).Description("The name of the user's business (if they are a service provider)");
            descriptor.Field(u => u.BusinessDescription).Description("Description of the user's business (if they are a service provider)");
            descriptor.Field(u => u.CreatedAt).Description("When the user was created");
            descriptor.Field(u => u.UpdatedAt).Description("When the user was last updated");

            // Exclude sensitive fields
            descriptor.Ignore(u => u.PasswordHash);
            descriptor.Ignore(u => u.SecurityStamp);
            descriptor.Ignore(u => u.ConcurrencyStamp);
            descriptor.Ignore(u => u.LockoutEnd);
            descriptor.Ignore(u => u.LockoutEnabled);
            descriptor.Ignore(u => u.AccessFailedCount);
            descriptor.Ignore(u => u.TwoFactorEnabled);
            descriptor.Ignore(u => u.PhoneNumberConfirmed);
            descriptor.Ignore(u => u.EmailConfirmed);
            descriptor.Ignore(u => u.NormalizedEmail);
            descriptor.Ignore(u => u.NormalizedUserName);
        }
    }
} 