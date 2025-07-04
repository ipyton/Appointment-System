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
    [Route("[controller]")]
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
                    filterParts.Add($"type eq '{type}'");
                }
                if (isServiceProvider.HasValue)
                {
                    filterParts.Add($"isServiceProvider eq {isServiceProvider.Value.ToString().ToLower()}");
                }
                
                if (isActive.HasValue)
                {
                    filterParts.Add($"isActive eq {isActive.Value.ToString().ToLower()}");
                }
                
                string filter = filterParts.Count > 0 ? string.Join(" and ", filterParts) : null;
                
                // Perform search
                var searchResults = await _searchService.SearchAsync(
                    query, 
                    filter, 
                    skip, 
                    top,
                    "id", "type", "name", "description", "isActive", "createdAt", "email", 
                    "isServiceProvider", "businessName", "price", "durationMinutes", "tags");
                
                // Format results
                var results = new
                {
                    TotalCount = searchResults.TotalCount,
                    Results = searchResults.GetResults().Select(result => 
                    {
                        var doc = result.Document;
                        return new
                        {
                            Id = doc.TryGetValue("id", out var id) ? id : null,
                            Type = doc.TryGetValue("type", out var type) ? type : null,
                            Name = doc.TryGetValue("name", out var name) ? name : null,
                            Description = doc.TryGetValue("description", out var description) ? description : null,
                            IsActive = doc.TryGetValue("isActive", out var isActive) ? isActive : null,
                            CreatedAt = doc.TryGetValue("createdAt", out var createdAt) ? createdAt : null,
                            Email = doc.TryGetValue("email", out var email) ? email : null,
                            IsServiceProvider = doc.TryGetValue("isServiceProvider", out var isServiceProvider) ? isServiceProvider : null,
                            BusinessName = doc.TryGetValue("businessName", out var businessName) ? businessName : null,
                            Price = doc.TryGetValue("price", out var price) ? price : null,
                            DurationMinutes = doc.TryGetValue("durationMinutes", out var durationMinutes) ? durationMinutes : null,
                            Tags = doc.TryGetValue("tags", out var tags) ? tags : null
                        };
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
            [FromQuery] string q,
            [FromQuery] bool fuzzy = true,
            [FromQuery] int top = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { Error = "Query parameter is required" });
                }
                
                var suggestResults = await _searchService.SuggestAsync(q, "sg", fuzzy, top);
                
                var suggestions = suggestResults.Results.Select(result => 
                {
                    var doc = result.Document;
                    return new
                    {
                        Text = result.Text,
                        Id = doc.TryGetValue("id", out var id) ? id : null,
                        Type = doc.TryGetValue("type", out var type) ? type : null,
                        Name = doc.TryGetValue("name", out var name) ? name : null,
                        Description = doc.TryGetValue("description", out var description) ? description : null
                    };
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