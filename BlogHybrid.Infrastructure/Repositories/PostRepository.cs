using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================================
        // Query methods - Basic
        // ========================================

        public async Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public async Task<Post?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Post?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public async Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // ========================================
        // Query methods - Advanced
        // ========================================

        /// <summary>
        /// Get IQueryable for advanced queries (used by Handlers)
        /// </summary>
        public IQueryable<Post> GetQueryable()
        {
            return _context.Posts.AsQueryable();
        }

        public async Task<List<Post>> GetByAuthorAsync(string authorId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetByCommunityAsync(int communityId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.CommunityId == communityId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetPublishedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.IsPublished && p.IsFeatured)
                .OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetPopularAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.ViewCount)
                .ThenByDescending(p => p.LikeCount)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        // ========================================
        // Query methods - Paged
        // ========================================

        public async Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(
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
            CancellationToken cancellationToken = default)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(lowerSearchTerm) ||
                    (p.Excerpt != null && p.Excerpt.ToLower().Contains(lowerSearchTerm)) ||
                    (p.Content != null && p.Content.ToLower().Contains(lowerSearchTerm))
                );
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (communityId.HasValue)
            {
                query = query.Where(p => p.CommunityId == communityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(authorId))
            {
                query = query.Where(p => p.AuthorId == authorId);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(p => p.IsPublished == isPublished.Value);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == isFeatured.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.Title)
                    : query.OrderByDescending(p => p.Title),
                "publishedat" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.PublishedAt)
                    : query.OrderByDescending(p => p.PublishedAt),
                "viewcount" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.ViewCount)
                    : query.OrderByDescending(p => p.ViewCount),
                "likecount" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.LikeCount)
                    : query.OrderByDescending(p => p.LikeCount),
                _ => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
            };

            // Apply pagination
            var posts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (posts, totalCount);
        }

        // ========================================
        // Command methods
        // ========================================

        public async Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
        {
            await _context.Posts.AddAsync(post, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return post;
        }

        public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ========================================
        // Utility methods
        // ========================================

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Posts.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Posts.Where(p => p.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task IncrementViewCountAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _context.Posts.FindAsync(new object[] { id }, cancellationToken);
            if (post != null)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts.CountAsync(cancellationToken);
        }

        public async Task<int> GetPublishedCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts.CountAsync(p => p.IsPublished, cancellationToken);
        }
    }
}