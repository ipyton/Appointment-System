using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Appointment_System.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Appointment_System.Services.Mappers;

namespace Appointment_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServicesController> _logger;
        private readonly SearchIndexingEventHandler _searchIndexingHandler;

        public ServicesController(
            ApplicationDbContext context,
            ILogger<ServicesController> logger,
            SearchIndexingEventHandler searchIndexingHandler
        )
        {
            _context = context;
            _logger = logger;
            _searchIndexingHandler = searchIndexingHandler;
        }

        // GET: api/Services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services.Where(s => s.IsActive).ToListAsync();
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return service;
        }

        // GET: api/Services/provider/{providerId}
        [HttpGet("provider/{providerId}")]
        public async Task<ActionResult<IEnumerable<Service>>> GetProviderServices(string providerId)
        {
            return await _context.Services.Where(s => s.ProviderId == providerId).ToListAsync();
        }

        [HttpPost("create")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> CreateService([FromBody] ServiceCreationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current user's ID (provider ID)
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
            {
                return Unauthorized("User not authenticated properly");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create and save the service
                var service = ServiceMapper.MapToService(dto, providerId);
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                // Create and save arrangements
                var arrangements = ServiceMapper.MapToArrangements(dto, service.Id);
                if (arrangements.Any())
                {
                    _context.Arrangements.AddRange(arrangements);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Return the created service with its arrangements
                return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(
                    500,
                    $"An error occurred while creating the service: {ex.Message}"
                );
            }
        }

        // POST: api/Services
        [HttpPost]
        [Authorize(Roles = "ServiceProvider,Admin")]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            // Ensure the current user is the provider or an admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (service.ProviderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            service.CreatedAt = DateTime.UtcNow;
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Index the new service in Azure Search
            await _searchIndexingHandler.ServiceCreatedOrUpdatedAsync(service);

            return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
        }

        // PUT: api/Services/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceProvider,Admin")]
        public async Task<IActionResult> UpdateService(int id, Service service)
        {
            if (id != service.Id)
            {
                return BadRequest();
            }

            // Ensure the current user is the provider or an admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var existingService = await _context.Services.FindAsync(id);

            if (existingService == null)
            {
                return NotFound();
            }

            if (existingService.ProviderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Update only allowed fields
            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.Price = service.Price;
            existingService.IsActive = service.IsActive;
            existingService.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                // Index the updated service in Azure Search
                await _searchIndexingHandler.ServiceCreatedOrUpdatedAsync(existingService);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Services/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceProvider,Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Ensure the current user is the provider or an admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (service.ProviderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            // Remove the service from Azure Search
            await _searchIndexingHandler.ServiceDeletedAsync(id);

            return NoContent();
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
