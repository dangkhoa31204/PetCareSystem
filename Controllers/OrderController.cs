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
    public class OrderController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public OrderController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tính tổng số lượng và doanh thu của tất cả các đơn hàng đã thanh toán (Paid)
        /// </summary>
        [HttpGet("total-paid")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalPaidOrders()
        {
            var paidOrders = await _context.Orders
                .Where(o => o.PaymentStatus == (int)PaymentStatus.Paid)
                .ToListAsync();

            var totalCount = paidOrders.Count;
            var totalAmountSum = paidOrders.Sum(o => o.TotalAmount);
            var finalAmountSum = paidOrders.Sum(o => o.FinalAmount);

            return Ok(new
            {
                TotalCount = totalCount,
                TotalAmount = totalAmountSum,
                FinalAmount = finalAmountSum
            });
        }

        /// <summary>
        /// [Customer] Lấy danh sách tất cả các đơn hàng của người dùng hiện tại
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDto>>> GetMyOrders()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var orderDtos = orders.Select(o => MapToOrderResponseDto(o)).ToList();
            return Ok(orderDtos);
        }

        /// <summary>
        /// [Customer] Lấy chi tiết một đơn hàng bằng ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Order not found");

            return Ok(MapToOrderResponseDto(order));
        }

        /// <summary>
        /// [Customer] Tạo một đơn hàng mới. Trạng thái ban đầu là 'Pending' và 'Unpaid'.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!createOrderDto.Items.Any())
                return BadRequest("Order must have at least one item");

            // Verify all products exist and have stock
            var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive == true)
                .ToListAsync();

            if (products.Count != productIds.Count)
                return BadRequest("One or more products not found or inactive");

            // Check stock
            foreach (var item in createOrderDto.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Insufficient stock for product: {product.Name}");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                BookingId = createOrderDto.BookingId > 0 ? createOrderDto.BookingId : null,
                OrderCode = GenerateOrderCode(),
                OrderType = (int)OrderType.Product,
                Status = (int)OrderStatus.Pending,
                PaymentStatus = (int)PaymentStatus.Unpaid,
                ShippingAddress = createOrderDto.ShippingAddress,
                DiscountAmount = createOrderDto.DiscountAmount ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            // Add order items
            decimal totalAmount = 0;
            foreach (var item in createOrderDto.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    SubTotal = product.Price * item.Quantity
                };
                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.SubTotal;

                // Reduce product stock
                product.StockQuantity -= item.Quantity;
                _context.Products.Update(product);
            }

            order.TotalAmount = totalAmount;

            // Check if user is pro member and apply discount
            var activeMembership = _context.UserMemberships
                .Where(m => m.UserId == userId && 
                           m.Status == (int)UserMembershipStatus.Active &&
                           m.EndDate > DateTime.Now)
                .FirstOrDefault();

            decimal proDiscount = 0;
            if (activeMembership != null)
            {
                proDiscount = totalAmount * 0.20m; // 20% discount for pro members on products
                order.DiscountAmount = (order.DiscountAmount ?? 0) + proDiscount;
            }

            order.FinalAmount = totalAmount - (order.DiscountAmount ?? 0);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, MapToOrderResponseDto(order));
        }

        /// <summary>
        /// [Customer] Cập nhật một đơn hàng (chỉ khi trạng thái là 'Pending' và 'Unpaid')
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(long id, CreateOrderDto updateOrderDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != (int)OrderStatus.Pending || order.PaymentStatus != (int)PaymentStatus.Unpaid)
                return BadRequest("Only pending and unpaid orders can be updated");

            // Restore previous stock
            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
                _context.Products.Update(item.Product);
            }

            // Verify all products exist and have stock
            var productIds = updateOrderDto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive == true)
                .ToListAsync();

            if (products.Count != productIds.Count)
                return BadRequest("One or more products not found or inactive");

            // Check stock
            foreach (var item in updateOrderDto.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Insufficient stock for product: {product.Name}");
            }

            // Remove old items
            _context.OrderItems.RemoveRange(order.OrderItems);

            // Add new items
            decimal totalAmount = 0;
            foreach (var item in updateOrderDto.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    SubTotal = product.Price * item.Quantity
                };
                _context.OrderItems.Add(orderItem);
                totalAmount += orderItem.SubTotal;

                // Reduce product stock
                product.StockQuantity -= item.Quantity;
                _context.Products.Update(product);
            }

            order.TotalAmount = totalAmount;
            order.DiscountAmount = updateOrderDto.DiscountAmount ?? 0;
            order.FinalAmount = totalAmount - (order.DiscountAmount ?? 0);
            order.ShippingAddress = updateOrderDto.ShippingAddress ?? order.ShippingAddress;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok("Order updated successfully");
        }

        /// <summary>
        /// [Customer] Hủy một đơn hàng (chỉ khi trạng thái là 'Pending' và 'Unpaid')
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != (int)OrderStatus.Pending || order.PaymentStatus != (int)PaymentStatus.Unpaid)
                return BadRequest("Only pending and unpaid orders can be cancelled");

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
                _context.Products.Update(item.Product);
            }

            order.Status = (int)OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok("Order cancelled successfully");
        }

        private OrderResponseDto MapToOrderResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                OrderType = order.OrderType,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems?.Select(oi => new OrderItemResponseDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubTotal = oi.SubTotal
                }).ToList() ?? new List<OrderItemResponseDto>()
            };
        }

        private string GenerateOrderCode()
        {
            return "ORD" + DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(1000, 9999);
        }

        private long GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
                return userId;
            return 0;
        }
    }
}
