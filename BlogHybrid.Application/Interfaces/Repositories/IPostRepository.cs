using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface IPostRepository
    {
        // Query methods - Basic
        Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Post?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<Post?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default);

        // Query methods - Advanced
        IQueryable<Post> GetQueryable(); // ← สำคัญ! สำหรับ Handler
        Task<List<Post>> GetByAuthorAsync(string authorId, CancellationToken cancellationToken = default);
        Task<List<Post>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<List<Post>> GetByCommunityAsync(int communityId, CancellationToken cancellationToken = default);
        Task<List<Post>> GetPublishedAsync(CancellationToken cancellationToken = default);
        Task<List<Post>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<List<Post>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<List<Post>> GetPopularAsync(int count = 10, CancellationToken cancellationToken = default);

        // Query methods - Paged
        Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? categoryId = null,
            int? communityId = null,
            string? authorId = null,
            bool? isPublished = null,
            bool? isFeatured = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default);

        // Command methods
        Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);
        Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
        Task DeleteAsync(Post post, CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
        Task IncrementViewCountAsync(int id, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetPublishedCountAsync(CancellationToken cancellationToken = default);
    }
}