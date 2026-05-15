using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Customer
{
    public class UpdateCustomerProfileDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
    }
}
