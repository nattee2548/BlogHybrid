using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface ICommunityRepository
    {
        // Query methods
        Task<Community?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Community?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<Community?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Community?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Community>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<List<Community>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);

        Task<(List<Community> Communities, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? categoryId = null,
            string? searchTerm = null,
            bool? isPrivate = null,
            bool? isActive = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default);

        // Command methods
        Task<Community> AddAsync(Community community, CancellationToken cancellationToken = default);
        Task UpdateAsync(Community community, CancellationToken cancellationToken = default);
        Task DeleteAsync(Community community, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task RestoreAsync(int id, CancellationToken cancellationToken = default);

        // Member management
        Task<CommunityMember?> GetMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default);
        Task<List<CommunityMember>> GetMembersAsync(int communityId, CancellationToken cancellationToken = default);
        Task<(List<CommunityMember> Members, int TotalCount)> GetMembersPagedAsync(
            int communityId,
            int pageNumber,
            int pageSize,
            CommunityRole? roleFilter = null,
            CancellationToken cancellationToken = default);
        Task AddMemberAsync(CommunityMember member, CancellationToken cancellationToken = default);
        Task UpdateMemberAsync(CommunityMember member, CancellationToken cancellationToken = default);
        Task RemoveMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default);
        Task<bool> IsMemberAsync(int communityId, string userId, CancellationToken cancellationToken = default);
        Task<bool> IsModeratorOrAdminAsync(int communityId, string userId, CancellationToken cancellationToken = default);

        // Invite management
        Task<CommunityInvite?> GetInviteByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<List<CommunityInvite>> GetCommunityInvitesAsync(int communityId, CancellationToken cancellationToken = default);
        Task AddInviteAsync(CommunityInvite invite, CancellationToken cancellationToken = default);
        Task UpdateInviteAsync(CommunityInvite invite, CancellationToken cancellationToken = default);
        Task<bool> HasPendingInviteAsync(int communityId, string email, CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<int> GetUserCommunityCountAsync(string userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<List<Community>> GetUserCommunitiesAsync(string userId, CancellationToken cancellationToken = default);
        Task<int> GetMemberCountAsync(int communityId, CancellationToken cancellationToken = default);
        Task<int> GetPostCountAsync(int communityId, CancellationToken cancellationToken = default);
        Task IncrementMemberCountAsync(int communityId, CancellationToken cancellationToken = default);
        Task DecrementMemberCountAsync(int communityId, CancellationToken cancellationToken = default);
        Task IncrementPostCountAsync(int communityId, CancellationToken cancellationToken = default);
        Task DecrementPostCountAsync(int communityId, CancellationToken cancellationToken = default);

        // Soft delete cleanup
        Task<List<Community>> GetCommunitiesForPermanentDeleteAsync(int retentionDays, CancellationToken cancellationToken = default);
    }
}