using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Admin;
using PetCareSystem.API.Models;

namespace PetCareSystem.API.Controllers
{
    [Route("api/admin/services")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminServiceController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public AdminServiceController(PetCareSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceManagementDto>>> GetServices()
        {
            var services = await _context.Services
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var result = services.Select(MapToServiceManagementDto).ToList();
            return Ok(result);
        }

        [HttpGet("{serviceId:long}")]
        public async Task<ActionResult<ServiceManagementDto>> GetService(long serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound("Service not found");
            }

            return Ok(MapToServiceManagementDto(service));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceManagementDto>> CreateService(CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationMinutes = dto.DurationMinutes,
                Category = dto.Category.HasValue ? (int)dto.Category.Value : null,
                ThumbnailUrl = dto.ThumbnailUrl,
                IsActive = dto.IsActive ?? true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { serviceId = service.ServiceId }, MapToServiceManagementDto(service));
        }

        [HttpPut("{serviceId:long}")]
        public async Task<IActionResult> UpdateService(long serviceId, UpdateServiceDto dto)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound("Service not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                service.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                service.Description = dto.Description;
            }

            if (dto.Price.HasValue)
            {
                service.Price = dto.Price.Value;
            }

            if (dto.DurationMinutes.HasValue)
            {
                service.DurationMinutes = dto.DurationMinutes.Value;
            }

            if (dto.Category.HasValue)
            {
                service.Category = (int)dto.Category.Value;
            }

            if (dto.ThumbnailUrl != null)
            {
                service.ThumbnailUrl = dto.ThumbnailUrl;
            }

            if (dto.IsActive.HasValue)
            {
                service.IsActive = dto.IsActive.Value;
            }

            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Service updated successfully");
        }

        [HttpDelete("{serviceId:long}")]
        public async Task<IActionResult> DeleteService(long serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound("Service not found");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok("Service deleted successfully");
        }

        private static ServiceManagementDto MapToServiceManagementDto(Service service)
        {
            return new ServiceManagementDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                DurationMinutes = service.DurationMinutes,
                Category = service.Category.HasValue ? (Enums.ServiceCategory)service.Category.Value : null,
                ThumbnailUrl = service.ThumbnailUrl,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }
    }
}
