namespace BlogHybrid.Application.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TokenDto? Token { get; set; }
    }
}