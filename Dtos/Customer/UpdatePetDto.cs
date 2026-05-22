namespace PetCareSystem.API.Dtos.Customer
{
    public class UpdatePetDto
    {
        public string? Name { get; set; }

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
