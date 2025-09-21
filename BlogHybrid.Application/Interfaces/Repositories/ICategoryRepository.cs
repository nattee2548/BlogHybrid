using BlogHybrid.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        // Query methods
        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<(List<Category> Categories, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null,
            string sortBy = "SortOrder",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default);

        // Command methods
        Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
        Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
        Task DeleteAsync(Category category, CancellationToken cancellationToken = default);

        // Utility methods
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<int> GetPostCountAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<int> GetMaxSortOrderAsync(CancellationToken cancellationToken = default);

        // Unit of Work
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
