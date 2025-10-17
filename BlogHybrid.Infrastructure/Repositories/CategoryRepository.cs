using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Category> Categories, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null,
            string sortBy = "SortOrder",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) ||
                                       c.Description.Contains(searchTerm));
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
                "createdat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),
                "isactive" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.IsActive)
                    : query.OrderBy(c => c.IsActive),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.SortOrder)
                    : query.OrderBy(c => c.SortOrder)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (categories, totalCount);
        }

        public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Add(category);
            return category;
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Remove(category);
            await Task.CompletedTask;
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.Where(c => c.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetPostCountAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Where(p => p.CategoryId == categoryId)
                .CountAsync(cancellationToken);
        }
        public async Task<int> GetCommunityCountAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Communities
                .Where(c => c.CategoryId == categoryId && !c.IsDeleted)
                .CountAsync(cancellationToken);
        }
        public async Task<int> GetMaxSortOrderAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .MaxAsync(c => (int?)c.SortOrder, cancellationToken) ?? 0;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
