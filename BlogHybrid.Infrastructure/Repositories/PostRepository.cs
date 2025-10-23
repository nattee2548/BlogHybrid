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

        #region Query Methods

        public async Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
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

        public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
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

        public async Task<List<Post>> GetPublishedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Post> Posts, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? categoryId = null,
            int? communityId = null,
            string? searchTerm = null,
            bool? isPublished = null,
            string sortBy = "CreatedAt",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .AsQueryable();

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by community
            if (communityId.HasValue)
            {
                query = query.Where(p => p.CommunityId == communityId.Value);
            }

            // Filter by published status
            if (isPublished.HasValue)
            {
                query = query.Where(p => p.IsPublished == isPublished.Value);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm) ||
                    p.Excerpt.Contains(searchTerm)
                );
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Title)
                    : query.OrderBy(p => p.Title),
                "publishedat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                    : query.OrderBy(p => p.PublishedAt ?? p.CreatedAt),
                "viewcount" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.ViewCount)
                    : query.OrderBy(p => p.ViewCount),
                "likecount" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.LikeCount)
                    : query.OrderBy(p => p.LikeCount),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };

            // Pagination
            var posts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (posts, totalCount);
        }

        public async Task<List<Post>> GetByCategoryIdAsync(int categoryId, int limit = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetByCommunityIdAsync(int communityId, int limit = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Where(p => p.CommunityId == communityId && p.IsPublished)
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Post>> GetByAuthorIdAsync(string authorId, int limit = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region Command Methods

        public async Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
        {
            await _context.Posts.AddAsync(post, cancellationToken);
            return post;
        }

        public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.Posts.Update(post);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
        {
            _context.Posts.Remove(post);
            await Task.CompletedTask;
        }

        #endregion

        #region Utility Methods

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Posts.Where(p => p.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task IncrementViewCountAsync(int postId, CancellationToken cancellationToken = default)
        {
            var post = await GetByIdAsync(postId, cancellationToken);
            if (post != null)
            {
                post.ViewCount++;
                await UpdateAsync(post, cancellationToken);
            }
        }

        public async Task<int> GetCommentCountAsync(int postId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId, cancellationToken);
        }

        public async Task<int> GetLikeCountAsync(int postId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PostLike>()
                .CountAsync(pl => pl.PostId == postId, cancellationToken);
        }

        #endregion
    }
}