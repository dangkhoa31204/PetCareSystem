using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public ServiceController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Public] Lấy danh sách tất cả các dịch vụ đang hoạt động
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ServiceDto>>> GetAllServices()
        {
            var services = await _context.Services
                .Where(s => s.IsActive == true)
                .Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    Category = s.Category,
                    ThumbnailUrl = s.ThumbnailUrl
                })
                .ToListAsync();

            return Ok(services);
        }

        /// <summary>
        /// [Public] Lấy chi tiết một dịch vụ bằng ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceDto>> GetService(long id)
        {
            var service = await _context.Services
                .Where(s => s.ServiceId == id && s.IsActive == true)
                .FirstOrDefaultAsync();

            if (service == null)
                return NotFound("Service not found");

            var serviceDto = new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                DurationMinutes = service.DurationMinutes,
                Category = service.Category,
                ThumbnailUrl = service.ThumbnailUrl
            };

            return Ok(serviceDto);
        }

        /// <summary>
        /// [Public] Lấy danh sách dịch vụ theo danh mục (Category)
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ServiceDto>>> GetServicesByCategory(int category)
        {
            var services = await _context.Services
                .Where(s => s.Category == category && s.IsActive == true)
                .Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    Category = s.Category,
                    ThumbnailUrl = s.ThumbnailUrl
                })
                .ToListAsync();

            return Ok(services);
        }
    }
}
