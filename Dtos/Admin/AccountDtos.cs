using PetCareSystem.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace PetCareSystem.API.Dtos.Admin
{
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public AccountRole Role { get; set; }
        public AccountStatus? Status { get; set; }
    }

    public class UpdateAccountDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public AccountRole? Role { get; set; }
        public AccountStatus? Status { get; set; }
    }

    public class AccountResponseDto
    {
        public long AccountId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public AccountRole Role { get; set; }
        public AccountStatus? Status { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
