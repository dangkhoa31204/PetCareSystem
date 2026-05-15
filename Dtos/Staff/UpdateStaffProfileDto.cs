using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Staff
{
    public class UpdateStaffProfileDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public string Department { get; set; }
    }
}
