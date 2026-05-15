using Microsoft.Extensions.Configuration;
using PetCareSystem.API.Dtos.Payment;
using PetCareSystem.API.Helpers;
using PetCareSystem.API.Models;
using System;
using System.Linq;

namespace PetCareSystem.API.Services.Vnpay
{
    public class VnpayService : IVnpayService
    {
        private readonly IConfiguration _config;
        private readonly PetCareSystemContext _context;

        public VnpayService(IConfiguration config, PetCareSystemContext context)
        {
            _config = config;
            _context = context;
        }

        public string CreatePaymentUrl(CreatePaymentDto model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnpayLibrary();
            decimal amount = 0;
            string orderInfo;

            if (model.OrderId.HasValue)
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == model.OrderId.Value);
                if (order == null) throw new Exception("Order not found");
                amount = order.FinalAmount;
                orderInfo = $"Thanh toan don hang {order.OrderCode}";
            }
            else if (model.BookingId.HasValue)
            {
                var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == model.BookingId.Value);
                if (booking == null) throw new Exception("Booking not found");
                amount = booking.TotalPrice ?? 0;
                orderInfo = $"Thanh toan lich hen {booking.BookingCode}";
            }
            else
            {
                throw new Exception("Either OrderId or BookingId must be provided.");
            }

            pay.AddRequestData("vnp_Version", _config["Vnpay:Version"] ?? "2.1.0");
            pay.AddRequestData("vnp_Command", _config["Vnpay:Command"] ?? "pay");
            pay.AddRequestData("vnp_TmnCode", _config["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _config["Vnpay:CurrCode"] ?? "VND");
            pay.AddRequestData("vnp_IpAddr", VnpayHelper.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _config["Vnpay:Locale"] ?? "vn");
            pay.AddRequestData("vnp_OrderInfo", orderInfo);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", _config["Vnpay:ReturnUrl"]);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_config["Vnpay:BaseUrl"], _config["Vnpay:HashSecret"]);

            return paymentUrl;
        }
    }
}
