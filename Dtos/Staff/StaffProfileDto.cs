namespace PetCareSystem.API.Dtos.Staff
{
    public class StaffProfileDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public string Department { get; set; }
        public bool? IsActive { get; set; }
    }
}
