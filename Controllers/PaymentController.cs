using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Payment;
using PetCareSystem.API.Enums;
using PetCareSystem.API.Models;
using PetCareSystem.API.Services.Sepay;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ISepayService _sepayService;
        private readonly PetCareSystemContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ISepayService sepayService, PetCareSystemContext context, IConfiguration configuration)
        {
            _sepayService = sepayService;
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// [Customer] Tạo mã QR thanh toán SePay cho một đơn hàng hoặc lịch hẹn
        /// </summary>
        /// <remarks>
        /// Cung cấp `orderId` hoặc `bookingId` để tạo link thanh toán.
        /// </remarks>
        [HttpPost("create-payment")]
        [Authorize]
        public IActionResult CreatePayment([FromBody] CreatePaymentDto model)
        {
            try
            {
                var paymentUrl = _sepayService.CreatePaymentQrUrl(model);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [SePay] Webhook nhận thông báo xác nhận thanh toán từ SePay
        /// </summary>
        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookDto body)
        {
            try
            {
                /* 
                // WARNING: Tạm thời tắt xác thực API Key để frontend có thể gọi test trực tiếp.
                // BẮT BUỘC phải mở lại khi chạy Production để tránh lỗi bảo mật mua hàng 0đ.
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { message = "Missing or invalid authorization header" });
                }

                var key = authHeader.Substring("Apikey ".Length).Trim();
                var expectedKey = _configuration["Sepay:WebhookApiKey"];
                if (!string.IsNullOrEmpty(expectedKey) && expectedKey.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase))
                {
                    expectedKey = expectedKey.Substring("Apikey ".Length).Trim();
                }

                if (string.IsNullOrEmpty(expectedKey) || key != expectedKey)
                {
                    return Unauthorized(new { message = "Invalid API Key" });
                }
                */

                // Lấy nội dung cần kiểm tra mã thanh toán (Code hoặc Content)
                string textToMatch = !string.IsNullOrEmpty(body.Code) ? body.Code : body.Content;
                if (string.IsNullOrEmpty(textToMatch))
                {
                    return BadRequest(new { message = "Payment code or content is missing" });
                }

                // Trích xuất mã giao dịch PETCARE[orderCode]
                var match = Regex.Match(textToMatch, @"PETCARE(\d+)", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    return BadRequest(new { message = "Invalid payment content format" });
                }

                long orderCode = long.Parse(match.Groups[1].Value);

                // Giải mã orderCode để cập nhật đơn hàng hoặc lịch hẹn
                if (orderCode >= 100000000000 && orderCode < 200000000000)
                {
                    long orderId = orderCode - 100000000000;
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                    if (order != null)
                    {
                        order.PaymentStatus = (int)PaymentStatus.Paid;
                        order.Status = (int)OrderStatus.Processing;
                        order.UpdatedAt = DateTime.UtcNow;
                        _context.Orders.Update(order);

                        // Lưu thông tin thanh toán vào bảng Payments
                        var payment = new Payment
                        {
                            OrderId = order.OrderId,
                            PaymentMethod = "Sepay",
                            TransactionCode = body.ReferenceCode ?? body.Id.ToString(),
                            Amount = body.TransferAmount,
                            PaymentStatus = (int)PaymentStatus.Paid,
                            PaidAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Payments.Add(payment);
                    }
                }
                else if (orderCode >= 200000000000 && orderCode < 300000000000)
                {
                    long bookingId = orderCode - 200000000000;
                    var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                    if (booking != null)
                    {
                        booking.Status = (int)BookingStatus.Confirmed;
                        booking.UpdatedAt = DateTime.UtcNow;
                        _context.Bookings.Update(booking);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
