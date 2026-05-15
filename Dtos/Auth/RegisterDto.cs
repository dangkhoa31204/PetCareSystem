using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Phone { get; set; }
    }
}
