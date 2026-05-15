using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Admin
{
    public class UpdateAdminProfileDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
    }
}
