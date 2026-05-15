namespace PetCareSystem.API.Dtos.Payment
{
    public class CreatePaymentDto
    {
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
        public string? BankCode { get; set; }
    }
}
