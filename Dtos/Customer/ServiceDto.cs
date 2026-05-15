namespace PetCareSystem.API.Dtos.Customer
{
    public class ServiceDto
    {
        public long ServiceId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int? DurationMinutes { get; set; }

        public int? Category { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
