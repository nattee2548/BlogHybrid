// BlogHybrid.Application/Interfaces/Repositories/ITagRepository.cs
using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface ITagRepository
    {
        // Query methods
        Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(List<Tag> Tags, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string sortBy = "Name",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default);
        Task<List<Tag>> SearchAsync(
            string searchTerm,
            int limit = 10,
            CancellationToken cancellationToken = default);

        // Command methods
        Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<int> GetPostCountAsync(int tagId, CancellationToken cancellationToken = default);
        Task<bool> IsTagUsedAsync(int tagId, CancellationToken cancellationToken = default);
    }
}