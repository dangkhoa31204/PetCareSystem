namespace PetCareSystem.API.Dtos.Doctor
{
    public class DoctorProfileDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public string Specialization { get; set; }
        public string License { get; set; }
        public bool? IsActive { get; set; }
    }
}
