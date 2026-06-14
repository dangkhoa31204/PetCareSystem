using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; } = string.Empty;
    }
}
