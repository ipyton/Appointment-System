using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Appointment_System.Services
{
    public class SearchIndexingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SearchIndexingService> _logger;
        private readonly TimeSpan _indexingInterval = TimeSpan.FromMinutes(30);

        public SearchIndexingService(
            IServiceProvider serviceProvider,
            ILogger<SearchIndexingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Search Indexing Service is starting.");

            try
            {
                // Initial indexing on startup
                await IndexDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial indexing");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Search Indexing Service is running periodic indexing.");

                try
                {
                    await IndexDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic indexing");
                }

                await Task.Delay(_indexingInterval, stoppingToken);
            }

            _logger.LogInformation("Search Indexing Service is stopping.");
        }

        private async Task IndexDataAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var searchService = scope.ServiceProvider.GetRequiredService<AzureSearchService>();

            try
            {
                // Ensure the index exists
                await searchService.CreateOrUpdateIndexAsync();

                // Index users
                var users = await dbContext.Users.ToListAsync();
                await searchService.IndexUsersAsync(users);
                _logger.LogInformation("Indexed {Count} users", users.Count);

                // Index services
                var services = await dbContext.Services.ToListAsync();
                await searchService.IndexServicesAsync(services);
                _logger.LogInformation("Indexed {Count} services", services.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search indexing");
                throw;
            }
        }
    }
} 