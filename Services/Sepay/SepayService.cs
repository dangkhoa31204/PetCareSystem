using Microsoft.Extensions.Configuration;
using PetCareSystem.API.Dtos.Payment;
using PetCareSystem.API.Models;
using System;
using System.Linq;

namespace PetCareSystem.API.Services.Sepay
{
    public class SepayService : ISepayService
    {
        private readonly IConfiguration _config;
        private readonly PetCareSystemContext _context;

        public SepayService(IConfiguration config, PetCareSystemContext context)
        {
            _config = config;
            _context = context;
        }

        public string CreatePaymentQrUrl(CreatePaymentDto model)
        {
            long orderCode = 0;
            long amount = 0;

            if (model.OrderId.HasValue)
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == model.OrderId.Value);
                if (order == null) throw new Exception("Order not found");

                orderCode = 100000000000 + order.OrderId;
                amount = (long)order.FinalAmount;
            }
            else if (model.BookingId.HasValue)
            {
                var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == model.BookingId.Value);
                if (booking == null) throw new Exception("Booking not found");

                orderCode = 200000000000 + booking.BookingId;
                amount = (long)(booking.TotalPrice ?? 0);
            }
            else
            {
                throw new Exception("Either OrderId or BookingId must be provided.");
            }

            string bank = _config["Sepay:Bank"] ?? throw new Exception("Sepay Bank not configured");
            string accountNumber = _config["Sepay:AccountNumber"] ?? throw new Exception("Sepay AccountNumber not configured");

            // Format of description: PETCARE[orderCode] to easily parse later
            string description = $"PETCARE{orderCode}";

            // URL to generate VietQR image from SePay
            string qrUrl = $"https://qr.sepay.vn/img?acc={accountNumber}&bank={bank}&amount={amount}&des={description}";

            return qrUrl;
        }
    }
}
