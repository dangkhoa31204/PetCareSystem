namespace PetCareSystem.API.Dtos.Staff
{
    public class PetDetailDto
    {
        public long PetId { get; set; }

        public long UserId { get; set; }

        public string OwnerName { get; set; }

        public string Name { get; set; }

        public string? Species { get; set; }

        public string? Breed { get; set; }

        public int? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Color { get; set; }

        public decimal? CurrentWeight { get; set; }

        public decimal? PreviousWeight { get; set; }

        public DateTime? WeightUpdatedAt { get; set; }

        public string? HealthStatus { get; set; }

        public string? AllergyInfo { get; set; }

        public string? VaccinationInfo { get; set; }

        public string? MedicalHistory { get; set; }

        public string? AvatarUrl { get; set; }

        public bool? IsNeutered { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<PetWeightHistoryDto> WeightHistory { get; set; } = new();
    }
}
