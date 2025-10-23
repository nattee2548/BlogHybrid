using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface IPostRepository
    {
        // Query methods
        Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Post?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Post?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Post>> GetPublishedAsync(CancellationToken cancellationToken = default);

        Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? categoryId = null,
            int? communityId = null,
            string? searchTerm = null,
            bool? isPublished = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default);

        Task<List<Post>> GetByCategoryIdAsync(int categoryId, int limit = 10, CancellationToken cancellationToken = default);
        Task<List<Post>> GetByCommunityIdAsync(int communityId, int limit = 10, CancellationToken cancellationToken = default);
        Task<List<Post>> GetByAuthorIdAsync(string authorId, int limit = 10, CancellationToken cancellationToken = default);

        // Command methods
        Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);
        Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
        Task DeleteAsync(Post post, CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
        Task IncrementViewCountAsync(int postId, CancellationToken cancellationToken = default);
        Task<int> GetCommentCountAsync(int postId, CancellationToken cancellationToken = default);
        Task<int> GetLikeCountAsync(int postId, CancellationToken cancellationToken = default);
    }
}