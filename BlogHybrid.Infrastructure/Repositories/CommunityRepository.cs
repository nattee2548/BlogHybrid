using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using BlogHybrid.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Query Methods

        public async Task<Community?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Community?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.Creator)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Community?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<Community?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.Creator)
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<List<Community>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Communities.AsQueryable();

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .OrderBy(c => c.SortOrder)
                .ThenByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Community>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .Where(c => c.CommunityCategories.Any(cc => cc.CategoryId == categoryId) && c.IsActive)
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .OrderBy(c => c.SortOrder)
                .ThenByDescending(c => c.MemberCount)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Community> Communities, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? categoryId = null,
            string? searchTerm = null,
            bool? isPrivate = null,
            bool? isActive = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            var query = _context.Communities
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.Creator)
                .AsQueryable();

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CommunityCategories.Any(cc => cc.CategoryId == categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    c.Description.Contains(searchTerm));
            }

            if (isPrivate.HasValue)
            {
                query = query.Where(c => c.IsPrivate == isPrivate.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "membercount" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.MemberCount)
                    : query.OrderBy(c => c.MemberCount),
                "postcount" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.PostCount)
                    : query.OrderBy(c => c.PostCount),
                "sortorder" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.SortOrder)
                    : query.OrderBy(c => c.SortOrder),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var communities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (communities, totalCount);
        }

        #endregion

        #region Command Methods

        public async Task<Community> AddAsync(Community community, CancellationToken cancellationToken = default)
        {
            _context.Communities.Add(community);
            return community;
        }

        public async Task UpdateAsync(Community community, CancellationToken cancellationToken = default)
        {
            community.UpdatedAt = DateTime.UtcNow;
            _context.Communities.Update(community);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Community community, CancellationToken cancellationToken = default)
        {
            _context.Communities.Remove(community);
            await Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var community = await _context.Communities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (community != null)
            {
                community.IsDeleted = true;
                community.DeletedAt = DateTime.UtcNow;
                community.IsActive = false;
                await UpdateAsync(community, cancellationToken);
            }
        }

        public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var community = await _context.Communities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (community != null && community.IsDeleted)
            {
                community.IsDeleted = false;
                community.DeletedAt = null;
                community.IsActive = true;
                await UpdateAsync(community, cancellationToken);
            }
        }

        #endregion

        #region Member Management

        public async Task<CommunityMember?> GetMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityMembers
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userId, cancellationToken);
        }

        public async Task<List<CommunityMember>> GetMembersAsync(int communityId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityMembers
                .Where(cm => cm.CommunityId == communityId && !cm.IsBanned)
                .Include(cm => cm.User)
                .OrderByDescending(cm => cm.Role) // Admin, Moderator, Member
                .ThenBy(cm => cm.JoinedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<CommunityMember> Members, int TotalCount)> GetMembersPagedAsync(
            int communityId,
            int pageNumber,
            int pageSize,
            CommunityRole? roleFilter = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.CommunityMembers
                .Where(cm => cm.CommunityId == communityId && !cm.IsBanned)
                .Include(cm => cm.User)
                .AsQueryable();

            if (roleFilter.HasValue)
            {
                query = query.Where(cm => cm.Role == roleFilter.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var members = await query
                .OrderByDescending(cm => cm.Role)
                .ThenBy(cm => cm.JoinedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (members, totalCount);
        }

        public async Task AddMemberAsync(CommunityMember member, CancellationToken cancellationToken = default)
        {
            _context.CommunityMembers.Add(member);
            await Task.CompletedTask;
        }

        public async Task UpdateMemberAsync(CommunityMember member, CancellationToken cancellationToken = default)
        {
            _context.CommunityMembers.Update(member);
            await Task.CompletedTask;
        }

        public async Task RemoveMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default)
        {
            var member = await _context.CommunityMembers
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userId, cancellationToken);

            if (member != null)
            {
                _context.CommunityMembers.Remove(member);
            }
        }

        public async Task<bool> IsMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityMembers
                .AnyAsync(cm => cm.CommunityId == communityId &&
                               cm.UserId == userId &&
                               cm.IsApproved &&
                               !cm.IsBanned,
                          cancellationToken);
        }

        public async Task<bool> IsModeratorOrAdminAsync(int communityId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityMembers
                .AnyAsync(cm => cm.CommunityId == communityId &&
                               cm.UserId == userId &&
                               (cm.Role == CommunityRole.Moderator || cm.Role == CommunityRole.Admin) &&
                               !cm.IsBanned,
                          cancellationToken);
        }

        #endregion

        #region Invite Management

        public async Task<CommunityInvite?> GetInviteByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityInvites
                .Include(ci => ci.Community)
                .Include(ci => ci.Inviter)
                .FirstOrDefaultAsync(ci => ci.Token == token, cancellationToken);
        }

        public async Task<List<CommunityInvite>> GetCommunityInvitesAsync(int communityId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityInvites
                .Where(ci => ci.CommunityId == communityId)
                .Include(ci => ci.Inviter)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddInviteAsync(CommunityInvite invite, CancellationToken cancellationToken = default)
        {
            _context.CommunityInvites.Add(invite);
            await Task.CompletedTask;
        }

        public async Task UpdateInviteAsync(CommunityInvite invite, CancellationToken cancellationToken = default)
        {
            _context.CommunityInvites.Update(invite);
            await Task.CompletedTask;
        }

        public async Task<bool> HasPendingInviteAsync(int communityId, string email, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityInvites
                .AnyAsync(ci => ci.CommunityId == communityId &&
                               ci.InviteeEmail == email &&
                               !ci.IsUsed &&
                               ci.ExpiresAt > DateTime.UtcNow,
                          cancellationToken);
        }

        #endregion

        #region Utility Methods

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Communities
                .IgnoreQueryFilters() // รวม soft deleted
                .Where(c => c.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetUserCommunityCountAsync(string userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Communities.Where(c => c.CreatorId == userId);

            if (!includeDeleted)
            {
                // Query filter จะกรองให้อัตโนมัติ (IsDeleted = false)
                return await query.CountAsync(cancellationToken);
            }

            return await query.IgnoreQueryFilters().CountAsync(cancellationToken);
        }

        public async Task<List<Community>> GetUserCommunitiesAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .Where(c => c.CreatorId == userId)
                .Include(c => c.CommunityCategories)
                    .ThenInclude(cc => cc.Category)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetMemberCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            return await _context.CommunityMembers
                .CountAsync(cm => cm.CommunityId == communityId &&
                                 cm.IsApproved &&
                                 !cm.IsBanned,
                           cancellationToken);
        }

        public async Task<int> GetPostCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .CountAsync(p => p.CommunityId == communityId && p.IsPublished, cancellationToken);
        }

        public async Task IncrementMemberCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            var community = await GetByIdAsync(communityId, cancellationToken);
            if (community != null)
            {
                community.MemberCount++;
                await UpdateAsync(community, cancellationToken);
            }
        }

        public async Task DecrementMemberCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            var community = await GetByIdAsync(communityId, cancellationToken);
            if (community != null && community.MemberCount > 0)
            {
                community.MemberCount--;
                await UpdateAsync(community, cancellationToken);
            }
        }

        public async Task IncrementPostCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            var community = await GetByIdAsync(communityId, cancellationToken);
            if (community != null)
            {
                community.PostCount++;
                await UpdateAsync(community, cancellationToken);
            }
        }

        public async Task DecrementPostCountAsync(int communityId, CancellationToken cancellationToken = default)
        {
            var community = await GetByIdAsync(communityId, cancellationToken);
            if (community != null && community.PostCount > 0)
            {
                community.PostCount--;
                await UpdateAsync(community, cancellationToken);
            }
        }

        public async Task<List<Community>> GetCommunitiesForPermanentDeleteAsync(int retentionDays, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            return await _context.Communities
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted &&
                           c.DeletedAt.HasValue &&
                           c.DeletedAt.Value <= cutoffDate)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}