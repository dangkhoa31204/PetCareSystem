using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Dtos.Admin
{
    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? DurationMinutes { get; set; }
        public ServiceCategory? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateServiceDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? DurationMinutes { get; set; }
        public ServiceCategory? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ServiceManagementDto
    {
        public long ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? DurationMinutes { get; set; }
        public ServiceCategory? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
