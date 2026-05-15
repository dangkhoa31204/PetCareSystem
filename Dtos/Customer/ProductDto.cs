namespace PetCareSystem.API.Dtos.Customer
{
    public class ProductDto
    {
        public long ProductId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        public int? Category { get; set; }

        public string? Brand { get; set; }

        public decimal? Weight { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
