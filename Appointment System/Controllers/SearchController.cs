using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointment_System.Models;
using Appointment_System.Services;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly AzureSearchService _searchService;

        public SearchController(AzureSearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search for users and services
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string query = "",
            [FromQuery] string type = null,
            [FromQuery] bool? isServiceProvider = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int skip = 0,
            [FromQuery] int top = 20)
        {
            try
            {
                // Build filter
                var filterParts = new List<string>();
                
                if (!string.IsNullOrEmpty(type))
                {
                    filterParts.Add($"Type eq '{type}'");
                }
                
                if (isServiceProvider.HasValue)
                {
                    filterParts.Add($"IsServiceProvider eq {isServiceProvider.Value.ToString().ToLower()}");
                }
                
                if (isActive.HasValue)
                {
                    filterParts.Add($"IsActive eq {isActive.Value.ToString().ToLower()}");
                }
                
                string filter = filterParts.Count > 0 ? string.Join(" and ", filterParts) : null;
                
                // Perform search
                var searchResults = await _searchService.SearchAsync(
                    query, 
                    filter, 
                    skip, 
                    top,
                    "Id", "Type", "Name", "Description", "IsActive", "CreatedAt", "Email", 
                    "IsServiceProvider", "BusinessName", "Price", "DurationMinutes", "Tags");
                
                // Format results
                var results = new
                {
                    TotalCount = searchResults.TotalCount,
                    Results = searchResults.GetResults().Select(result => new
                    {
                        Id = result.Document.Id,
                        Type = result.Document.Type,
                        Name = result.Document.Name,
                        Description = result.Document.Description,
                        IsActive = result.Document.IsActive,
                        CreatedAt = result.Document.CreatedAt,
                        Email = result.Document.Email,
                        IsServiceProvider = result.Document.IsServiceProvider,
                        BusinessName = result.Document.BusinessName,
                        Price = result.Document.Price,
                        DurationMinutes = result.Document.DurationMinutes,
                        Tags = result.Document.Tags
                    }),
                    Facets = searchResults.Facets?.ToDictionary(
                        facet => facet.Key,
                        facet => facet.Value.Select(f => new { Value = f.Value, Count = f.Count }))
                };
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Search failed", Message = ex.Message });
            }
        }

        /// <summary>
        /// Get autocomplete suggestions
        /// </summary>
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(
            [FromQuery] string query,
            [FromQuery] bool fuzzy = true,
            [FromQuery] int top = 5)
        {
            try
            {
                var suggestResults = await _searchService.SuggestAsync(query, "sg", fuzzy, top);
                
                var suggestions = suggestResults.Results.Select(result => new
                {
                    Text = result.Text,
                    Id = result.Document.Id,
                    Type = result.Document.Type,
                    Name = result.Document.Name
                });
                
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Suggestion failed", Message = ex.Message });
            }
        }

        /// <summary>
        /// Manually trigger indexing of all users and services
        /// </summary>
        [HttpPost("index")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IndexAll([FromServices] Data.ApplicationDbContext dbContext)
        {
            try
            {
                // Create or update the index
                await _searchService.CreateOrUpdateIndexAsync();
                
                // Index all users
                var users = dbContext.Users.ToList();
                await _searchService.IndexUsersAsync(users);
                
                // Index all services
                var services = dbContext.Services.ToList();
                await _searchService.IndexServicesAsync(services);
                
                return Ok(new { Message = "Indexing completed", UsersCount = users.Count, ServicesCount = services.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Indexing failed", Message = ex.Message });
            }
        }
    }
} 