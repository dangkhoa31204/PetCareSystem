namespace PetCareSystem.API.Dtos.Customer
{
    public class CreatePetDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Species { get; set; }

        public string? Breed { get; set; }

        public int? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Color { get; set; }

        public decimal? CurrentWeight { get; set; }

        public string? HealthStatus { get; set; }

        public string? AvatarUrl { get; set; }

        public bool? IsNeutered { get; set; }
    }
}
