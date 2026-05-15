namespace PetCareSystem.API.Dtos.Auth
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
}
