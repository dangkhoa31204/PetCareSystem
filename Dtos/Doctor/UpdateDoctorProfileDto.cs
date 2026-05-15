using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Doctor
{
    public class UpdateDoctorProfileDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public string Specialization { get; set; }
        public string License { get; set; }
    }
}
