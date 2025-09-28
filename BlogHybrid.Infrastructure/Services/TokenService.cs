using BlogHybrid.Application.DTOs.Auth;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Configuration;
using BlogHybrid.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlogHybrid.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _context = context;
        }

        public async Task<TokenDto> GenerateTokenAsync(ApplicationUser user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedDate = DateTime.UtcNow,
                CreatedByIp = GetIpAddress()
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RefreshTokenExpiry = refreshTokenEntity.ExpiryDate,
                TokenType = "Bearer"
            };
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (!storedToken.IsActive)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = storedToken.IsExpired ? "Token expired" : "Token revoked"
                };
            }

            // Revoke old token
            storedToken.RevokedDate = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress ?? GetIpAddress();

            // Generate new tokens
            var newTokenDto = await GenerateTokenAsync(storedToken.User);

            // Mark old token as replaced
            storedToken.ReplacedByToken = newTokenDto.RefreshToken;

            await _context.SaveChangesAsync();

            return new RefreshTokenResult
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newTokenDto
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null, string? reason = null)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive)
                return false;

            storedToken.RevokedDate = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress ?? GetIpAddress();
            storedToken.ReasonRevoked = reason;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedDate = DateTime.UtcNow;
                token.ReasonRevoked = "Revoked all tokens";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false, // Don't validate lifetime for expired tokens
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("DisplayName", user.DisplayName)
            };

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GetIpAddress()
        {
            // This will be set properly when called from HTTP context
            return "Unknown";
        }
    }
}