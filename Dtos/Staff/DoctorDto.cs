namespace PetCareSystem.API.Dtos.Staff
{
    public class DoctorDto
    {
        public long UserId { get; set; }
        public long AccountId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Specialization { get; set; }
    }
}
