using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Enums;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetCareSystem.API.Controllers
{
    [Route("api/staff/ordermanagement")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class StaffOrderManagementController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public StaffOrderManagementController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng (có thể lọc theo trạng thái)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == (int)status.Value);
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            // In a real app, mapping to a DTO is recommended
            return Ok(orders);
        }

        /// <summary>
        /// Lấy chi tiết một đơn hàng
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }

        /// <summary>
        /// Cập nhật trạng thái của một đơn hàng
        /// </summary>
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            // You can add more complex validation logic for status transitions here
            order.Status = (int)dto.Status;
            order.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Order status updated successfully.");
        }
    }

    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}
