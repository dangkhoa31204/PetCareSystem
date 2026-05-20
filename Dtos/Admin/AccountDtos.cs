using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Dtos.Admin
{
    public class CreateAccountDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialization { get; set; }
        public AccountRole Role { get; set; }
        public AccountStatus? Status { get; set; }
    }

    public class UpdateAccountDto
    {
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
