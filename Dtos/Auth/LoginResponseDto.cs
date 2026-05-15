namespace PetCareSystem.API.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } = 604800; // 7 days in seconds
        public UserInfoDto User { get; set; }
    }

    public class UserInfoDto
    {
        public long UserId { get; set; }
        public long AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsProMember { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? ProExpiredAt { get; set; }
    }
}
