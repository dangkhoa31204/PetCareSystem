using Microsoft.AspNetCore.Http;
using PetCareSystem.API.Dtos.Payment;

namespace PetCareSystem.API.Services.Vnpay
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(CreatePaymentDto model, HttpContext context);
    }
}
