using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
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
        Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
        Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
        Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);

        // ========== Hierarchical Methods ==========
        /// <summary>
        /// ดึงหมวดหมู่หลักทั้งหมด (ParentCategoryId == null)
        /// </summary>
        Task<List<Category>> GetParentCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// ดึงหมวดหมู่ย่อยของหมวดหมู่หลัก
        /// </summary>
        Task<List<Category>> GetSubCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// ดึงหมวดหมู่พร้อมหมวดหมู่ย่อย (include SubCategories)
        /// </summary>
        Task<Category?> GetByIdWithSubCategoriesAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// ดึงหมวดหมู่ทั้งหมดแบบ hierarchical tree
        /// </summary>
        Task<List<Category>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// เช็คว่าหมวดหมู่มีหมวดหมู่ย่อยหรือไม่
        /// </summary>
        Task<bool> HasSubCategoriesAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// นับจำนวนหมวดหมู่ย่อย
        /// </summary>
        Task<int> CountSubCategoriesAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// ดึง SortOrder สูงสุดสำหรับกำหนดค่า SortOrder ใหม่
        /// </summary>
        Task<int> GetMaxSortOrderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// นับจำนวนโพสต์ในหมวดหมู่
        /// </summary>
        Task<int> GetPostCountAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// นับจำนวนชุมชนในหมวดหมู่
        /// </summary>
        Task<int> GetCommunityCountAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}