using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Post
{
    public class GetPublicPostsHandler : IRequestHandler<GetPublicPostsQuery, GetPublicPostsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPublicPostsHandler> _logger;

        public GetPublicPostsHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPublicPostsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GetPublicPostsResult> Handle(
            GetPublicPostsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Base Query: เฉพาะ Posts ที่เผยแพร่แล้วและไม่ถูกลบ
                var query = _unitOfWork.Posts.GetQueryable()
                    .Where(p => p.IsPublished && !p.IsDeleted)
                    .Include(p => p.Author)
                    .Include(p => p.Category)
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Filter: Search Term
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(searchTerm) ||
                        (p.Excerpt != null && p.Excerpt.ToLower().Contains(searchTerm)) ||
                        p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(searchTerm))
                    );
                }

                // Filter: Category
                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                // Filter: Community
                if (request.CommunityId.HasValue)
                {
                    query = query.Where(p => p.CommunityId == request.CommunityId.Value);
                }

                // Filter: Tag
                if (!string.IsNullOrWhiteSpace(request.Tag))
                {
                    query = query.Where(p => p.PostTags.Any(pt => pt.Tag.Name == request.Tag));
                }

                // Filter: Featured Only
                if (request.FeaturedOnly)
                {
                    query = query.Where(p => p.IsFeatured);
                }

                // Get Total Count
                var totalCount = await query.CountAsync(cancellationToken);

                // Sorting
                query = request.SortBy.ToLower() switch
                {
                    "viewcount" => request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.ViewCount)
                        : query.OrderByDescending(p => p.ViewCount),
                    "likecount" => request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.LikeCount)
                        : query.OrderByDescending(p => p.LikeCount),
                    "title" => request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.Title)
                        : query.OrderByDescending(p => p.Title),
                    _ => request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.PublishedAt)
                        : query.OrderByDescending(p => p.PublishedAt)
                };

                // Pagination
                var posts = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new PublicPostItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Slug = p.Slug,
                        Excerpt = p.Excerpt,
                        FeaturedImageUrl = p.FeaturedImageUrl,
                        PublishedAt = p.PublishedAt ?? p.CreatedAt,
                        IsFeatured = p.IsFeatured,
                        AuthorId = p.AuthorId,
                        AuthorName = p.Author.DisplayName ?? p.Author.UserName!,
                        AuthorProfileImageUrl = p.Author.ProfileImageUrl,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        CategoryColor = p.Category != null ? p.Category.Color : null,
                        CommunityId = p.CommunityId,
                        CommunityName = p.Community != null ? p.Community.Name : null,
                        CommunityImageUrl = p.Community != null ? p.Community.ImageUrl : null,
                        ViewCount = p.ViewCount,
                        LikeCount = p.LikeCount,
                        CommentCount = p.CommentCount,
                        Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Retrieved {Count} public posts (Page {Page}/{TotalPages})",
                    posts.Count, request.PageNumber, Math.Ceiling((double)totalCount / request.PageSize)
                );

                return new GetPublicPostsResult
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public posts");
                return new GetPublicPostsResult
                {
                    Posts = new List<PublicPostItem>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }
}