using BlogHybrid.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // Query methods
        Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<List<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? roleFilter = null,
            bool? isActive = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default);

        // Command methods
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);
        Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        // Role management
        Task<List<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
        Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default);

        // Password management
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string newPassword, CancellationToken cancellationToken = default);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        // Email and phone confirmation
        Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<IdentityResult> SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken = default);
        Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken = default);

        // External logins
        Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        // Claims
        Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        // Statistics
        Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetUsersCountByRoleAsync(string role, CancellationToken cancellationToken = default);
        Task<int> GetNewUsersCountAsync(DateTime fromDate, CancellationToken cancellationToken = default);
        Task<DateTime?> GetLastUserRegistrationDateAsync(CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> UserNameExistsAsync(string userName, string? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    }
}