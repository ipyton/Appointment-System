using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Appointment_System.Models;

namespace Appointment_System.Services
{
    public class SearchIndexingEventHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SearchIndexingEventHandler> _logger;

        public SearchIndexingEventHandler(
            IServiceProvider serviceProvider,
            ILogger<SearchIndexingEventHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task UserCreatedOrUpdatedAsync(ApplicationUser user)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<AzureSearchService>();
                await searchService.IndexUserAsync(user);
                _logger.LogInformation("Indexed user {UserId} in search", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing user {UserId} in search", user.Id);
            }
        }

        public async Task UserDeletedAsync(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<AzureSearchService>();
                await searchService.DeleteUserAsync(userId);
                _logger.LogInformation("Removed user {UserId} from search index", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from search index", userId);
            }
        }

        public async Task ServiceCreatedOrUpdatedAsync(Service service)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<AzureSearchService>();
                await searchService.IndexServiceAsync(service);
                _logger.LogInformation("Indexed service {ServiceId} in search", service.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing service {ServiceId} in search", service.Id);
            }
        }

        public async Task ServiceDeletedAsync(int serviceId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<AzureSearchService>();
                await searchService.DeleteServiceAsync(serviceId);
                _logger.LogInformation("Removed service {ServiceId} from search index", serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing service {ServiceId} from search index", serviceId);
            }
        }
    }
} 