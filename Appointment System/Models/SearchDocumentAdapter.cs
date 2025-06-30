using System;
using System.Collections.Generic;
using System.Linq;

namespace Appointment_System.Models
{
    public class SearchDocumentAdapter
    {
        /// <summary>
        /// Converts an ApplicationUser to a SearchDocument for Azure Search indexing
        /// </summary>
        public static SearchDocument FromApplicationUser(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
                
            var document = new SearchDocument
            {
                Id = $"user-{user.Id}",
                Type = "User",
                Name = user.FullName,
                Description = user.IsServiceProvider ? user.BusinessDescription : string.Empty,
                IsActive = user.EmailConfirmed, // Using EmailConfirmed as a proxy for active status
                CreatedAt = user.CreatedAt,
                
                // User specific fields
                Email = user.Email,
                Address = user.Address,
                IsServiceProvider = user.IsServiceProvider,
                BusinessName = user.BusinessName,
                
                // Add relevant tags
                Tags = new List<string>()
            };
            
            // Add tags based on user properties
            if (user.IsServiceProvider)
            {
                document.Tags.Add("ServiceProvider");
                if (!string.IsNullOrEmpty(user.BusinessName))
                {
                    document.Tags.Add($"Business:{user.BusinessName}");
                }
            }
            else
            {
                document.Tags.Add("Customer");
            }
            
            return document;
        }
        
        /// <summary>
        /// Converts a Service to a SearchDocument for Azure Search indexing
        /// </summary>
        public static SearchDocument FromService(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
                
            var document = new SearchDocument
            {
                Id = $"service-{service.Id}",
                Type = "Service",
                Name = service.Name,
                Description = service.Description,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                
                // Service specific fields
                Price = service.Price,
                DurationMinutes = service.DurationMinutes,
                ProviderId = service.ProviderId,
                
                // Add relevant tags
                Tags = new List<string>
                {
                    "Service",
                    $"Duration:{service.DurationMinutes}min"
                }
            };
            
            // Add price range tag
            if (service.Price < 50)
                document.Tags.Add("PriceRange:Budget");
            else if (service.Price < 100)
                document.Tags.Add("PriceRange:Standard");
            else if (service.Price < 200)
                document.Tags.Add("PriceRange:Premium");
            else
                document.Tags.Add("PriceRange:Luxury");
                
            return document;
        }
        
        /// <summary>
        /// Batch converts a collection of ApplicationUsers to SearchDocuments
        /// </summary>
        public static IEnumerable<SearchDocument> FromApplicationUsers(IEnumerable<ApplicationUser> users)
        {
            return users?.Select(FromApplicationUser) ?? Enumerable.Empty<SearchDocument>();
        }
        
        /// <summary>
        /// Batch converts a collection of Services to SearchDocuments
        /// </summary>
        public static IEnumerable<SearchDocument> FromServices(IEnumerable<Service> services)
        {
            return services?.Select(FromService) ?? Enumerable.Empty<SearchDocument>();
        }
    }
} 