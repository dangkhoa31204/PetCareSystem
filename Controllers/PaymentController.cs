using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareSystem.API.Dtos.Payment;
using PetCareSystem.API.Enums;
using PetCareSystem.API.Helpers;
using PetCareSystem.API.Models;
using PetCareSystem.API.Services.Vnpay;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnpayService _vnpayService;
        private readonly PetCareSystemContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(IVnpayService vnpayService, PetCareSystemContext context, IConfiguration configuration)
        {
            _vnpayService = vnpayService;
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// [Customer] Tạo URL thanh toán VNPay cho một đơn hàng hoặc lịch hẹn
        /// </summary>
        /// <remarks>
        /// Cung cấp `orderId` hoặc `bookingId` để tạo thanh toán.
        /// </remarks>
        [HttpPost("create-payment")]
        [Authorize]
        public IActionResult CreatePayment([FromBody] CreatePaymentDto model)
        {
            var paymentUrl = _vnpayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { PaymentUrl = paymentUrl });
        }

        /// <summary>
        /// [VNPay] Endpoint để VNPay gọi về sau khi người dùng hoàn tất thanh toán
        /// </summary>
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnpayReturn()
        {
            var vnpayData = new VnpayLibrary();
            var vnp_HashSecret = _configuration["Vnpay:HashSecret"];

            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpayData.AddResponseData(key, value);
                }
            }

            var vnp_TxnRef = vnpayData.GetResponseData("vnp_TxnRef");
            var vnp_ResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();

            bool checkSignature = vnpayData.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            if (!checkSignature)
            {
                return BadRequest("Invalid signature");
            }

            if (vnp_ResponseCode == "00")
            {
                // Payment successful
                var orderInfo = vnpayData.GetResponseData("vnp_OrderInfo");
                var amount = Convert.ToDecimal(vnpayData.GetResponseData("vnp_Amount")) / 100;

                // This is a simplified way to find the order/booking. 
                // In a real-world scenario, you might want to store the order/booking ID in vnp_TxnRef
                // and retrieve it here for more reliable updates.
                if (orderInfo.Contains("don hang"))
                {
                    var orderCode = orderInfo.Split(' ').Last();
                    var order = _context.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                    if (order != null)
                    {
                        order.PaymentStatus = (int)PaymentStatus.Paid;
                        order.Status = (int)OrderStatus.Processing;
                        _context.Orders.Update(order);
                    }
                }
                else if (orderInfo.Contains("lich hen"))
                {
                    var bookingCode = orderInfo.Split(' ').Last();
                    var booking = _context.Bookings.FirstOrDefault(b => b.BookingCode == bookingCode);
                    if (booking != null)
                    {
                        booking.Status = (int)BookingStatus.Confirmed;
                        // You might want to create a payment record for bookings as well
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("Payment successful");
            }
            else
            {
                // Payment failed
                return BadRequest("Payment failed");
            }
        }
    }
}
