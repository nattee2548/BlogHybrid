using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region Query Methods

        public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<List<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? roleFilter = null,
            bool? isActive = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            var query = _userManager.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(searchTerm) ||
                    u.UserName!.Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(searchTerm))
                );
            }

            // Apply active filter
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "email" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "username" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.UserName)
                    : query.OrderBy(u => u.UserName),
                "firstname" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName),
                "lastname" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName),
                "lastloginat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.LastLoginAt)
                    : query.OrderBy(u => u.LastLoginAt),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        #endregion

        #region Command Methods

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.DeleteAsync(user);
        }

        #endregion

        #region Role Management

        public async Task<List<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            return await _userManager.AddToRolesAsync(user, roles);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.ToList();
        }

        #endregion

        #region Password Management

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string newPassword, CancellationToken cancellationToken = default)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        #endregion

        #region Email and Phone

        public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IdentityResult> SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken = default)
        {
            return await _userManager.SetEmailAsync(user, email);
        }

        public async Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _userManager.SetPhoneNumberAsync(user, phoneNumber);
        }

        #endregion

        #region External Logins and Claims

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GetLoginsAsync(user);
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GetClaimsAsync(user);
        }

        #endregion

        #region Statistics

        public async Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users.CountAsync(cancellationToken);
        }

        public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users.CountAsync(u => u.IsActive, cancellationToken);
        }

        public async Task<int> GetUsersCountByRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.Count;
        }

        public async Task<int> GetNewUsersCountAsync(DateTime fromDate, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .CountAsync(u => u.CreatedAt >= fromDate, cancellationToken);
        }

        public async Task<DateTime?> GetLastUserRegistrationDateAsync(CancellationToken cancellationToken = default)
        {
            var lastUser = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return lastUser?.CreatedAt;
        }

        #endregion

        #region Utility Methods

        public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _userManager.Users.Where(u => u.Email == email);

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> UserNameExistsAsync(string userName, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _userManager.Users.Where(u => u.UserName == userName);

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}