using System.Threading.Tasks;
using PetCareSystem.API.Dtos.Payment;

namespace PetCareSystem.API.Services.Sepay
{
    public interface ISepayService
    {
        string CreatePaymentQrUrl(CreatePaymentDto model);
    }
}
