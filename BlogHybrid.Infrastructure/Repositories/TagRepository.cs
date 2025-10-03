// BlogHybrid.Infrastructure/Repositories/TagRepository.cs
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;

        public TagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        }

        public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Tag> Tags, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string sortBy = "Name",
            string sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            var query = _context.Tags.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) ||
                                       t.Slug.Contains(searchTerm));
            }

            query = sortBy.ToLower() switch
            {
                "createdat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),
                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.Name)
                    : query.OrderBy(t => t.Name)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var tags = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (tags, totalCount);
        }

        public async Task<List<Tag>> SearchAsync(
            string searchTerm,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Where(t => t.Name.Contains(searchTerm))
                .OrderBy(t => t.Name)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _context.Tags.Add(tag);
            return tag;
        }

        public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _context.Tags.Update(tag);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
        {
            _context.Tags.Remove(tag);
            await Task.CompletedTask;
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Tags.Where(t => t.Slug == slug);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetPostCountAsync(int tagId, CancellationToken cancellationToken = default)
        {
            return await _context.PostTags
                .Where(pt => pt.TagId == tagId)
                .CountAsync(cancellationToken);
        }

        public async Task<bool> IsTagUsedAsync(int tagId, CancellationToken cancellationToken = default)
        {
            return await _context.PostTags
                .AnyAsync(pt => pt.TagId == tagId, cancellationToken);
        }
    }
}