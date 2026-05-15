namespace PetCareSystem.API.Dtos.Staff
{
    public class PetWeightHistoryDto
    {
        public long WeightHistoryId { get; set; }

        public decimal Weight { get; set; }

        public DateTime? RecordedAt { get; set; }

        public string? Note { get; set; }
    }

    public class UpdatePetHealthDto
    {
        public decimal? CurrentWeight { get; set; }

        public string? HealthStatus { get; set; }

        public string? AllergyInfo { get; set; }

        public string? VaccinationInfo { get; set; }

        public string? MedicalHistory { get; set; }

        public bool? IsNeutered { get; set; }

        public string? WeightHistoryNote { get; set; }
    }
}
