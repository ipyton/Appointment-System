using System;
using System.Collections.Generic;
using System.Linq;
using Appointment_System.Models;

namespace Appointment_System.Services
{
    public static class SearchDocumentAdapter
    {
        public static Dictionary<string, object> FromApplicationUser(ApplicationUser user)
        {       
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new Dictionary<string, object>
            {
                ["id"] = $"user-{user.Id}",
                ["type"] = "User",
                ["name"] = user.FullName,
                ["description"] = user.BusinessDescription,
                ["email"] = user.Email,
                ["address"] = user.Address,
                ["isServiceProvider"] = user.IsServiceProvider,
                ["businessName"] = user.BusinessName,
                ["isActive"] = true,
                ["createdAt"] = user.CreatedAt,
                ["tags"] = GetUserTags(user)
            };
        }

        public static Dictionary<string, object> FromService(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return new Dictionary<string, object>
            {
                ["id"] = $"service-{service.Id}",
                ["type"] = "Service",
                ["name"] = service.Name,
                ["description"] = service.Description,
                ["price"] = service.Price,
                ["durationMinutes"] = service.DurationMinutes,
                ["providerId"] = service.ProviderId,
                ["isActive"] = service.IsActive,
                ["createdAt"] = service.CreatedAt,
                ["tags"] = GetServiceTags(service)
            };
        }

        public static IEnumerable<Dictionary<string, object>> FromApplicationUsers(IEnumerable<ApplicationUser> users)
        {
            if (users == null)
                return Enumerable.Empty<Dictionary<string, object>>();

            return users.Select(FromApplicationUser);
        }

        public static IEnumerable<Dictionary<string, object>> FromServices(IEnumerable<Service> services)
        {
            if (services == null)
                return Enumerable.Empty<Dictionary<string, object>>();

            return services.Select(FromService);
        }

        private static List<string> GetUserTags(ApplicationUser user)
        {
            var tags = new List<string>();

            if (user.IsServiceProvider)
                tags.Add("ServiceProvider");
            else
                tags.Add("Client");

            return tags;
        }

        private static List<string> GetServiceTags(Service service)
        {
            var tags = new List<string>();

            // Add price range tags
            if (service.Price < 50)
                tags.Add("PriceLow");
            else if (service.Price < 100)
                tags.Add("PriceMedium");
            else
                tags.Add("PriceHigh");

            // Add duration tags
            if (service.DurationMinutes <= 30)
                tags.Add("DurationShort");
            else if (service.DurationMinutes <= 60)
                tags.Add("DurationMedium");
            else
                tags.Add("DurationLong");

            if (service.IsActive)
                tags.Add("Active");

            return tags;
        }
    }
} 