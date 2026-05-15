namespace PetCareSystem.API.Dtos.Customer
{
    public class CustomerProfileDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public bool? IsProMember { get; set; }
        public DateTime? ProExpiredAt { get; set; }
    }
}
