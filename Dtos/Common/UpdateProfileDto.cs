namespace PetCareSystem.API.Dtos.Common
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? Specialization { get; set; }
        public string? Department { get; set; }
    }
}
