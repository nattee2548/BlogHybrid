// Path: BlogHybrid.Application/DTOs/Auth/RegisterUserDto.cs
namespace BlogHybrid.Application.DTOs.Auth
{
    /// <summary>
    /// DTO สำหรับ Request ลงทะเบียน
    /// </summary>
    public class RegisterUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool AcceptTerms { get; set; } = false;
    }

    /// <summary>
    /// DTO สำหรับ Response ลงทะเบียน
    /// </summary>
    public class RegisterUserResponseDto
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}