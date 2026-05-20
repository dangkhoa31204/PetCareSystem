namespace PetCareSystem.API.Dtos.Admin
{
    public class UpdateProPackagePriceDto
    {
        public decimal Price { get; set; }
        public decimal? ServiceDiscount { get; set; }
        public decimal? ProductDiscount { get; set; }
    }
}
