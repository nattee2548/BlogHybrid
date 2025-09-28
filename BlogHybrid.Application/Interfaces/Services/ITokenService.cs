using BlogHybrid.Application.DTOs.Auth;
using BlogHybrid.Domain.Entities;
using System.Security.Claims;

namespace BlogHybrid.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<TokenDto> GenerateTokenAsync(ApplicationUser user);
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null, string? reason = null);
        Task<bool> RevokeAllUserTokensAsync(string userId);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}