using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Handlers.Post
{
    public class GetUserPostsQueryHandler : IRequestHandler<GetUserPostsQuery, GetUserPostsResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserPostsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetUserPostsResult> Handle(GetUserPostsQuery request, CancellationToken cancellationToken)
        {
            // Get all posts by user
            var query = _unitOfWork.Posts.GetQueryable()
                .Where(p => p.AuthorId == request.UserId && !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) ||
                    (p.Excerpt != null && p.Excerpt.ToLower().Contains(searchTerm)) ||
                    p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(searchTerm))
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "all")
            {
                switch (request.StatusFilter.ToLower())
                {
                    case "published":
                        query = query.Where(p => p.IsPublished);
                        break;
                    case "draft":
                        query = query.Where(p => !p.IsPublished);
                        break;
                    case "featured":
                        query = query.Where(p => p.IsFeatured);
                        break;
                }
            }

            // Calculate statistics (before pagination)
            var allUserPosts = await _unitOfWork.Posts.GetQueryable()
                .Where(p => p.AuthorId == request.UserId)
                .ToListAsync(cancellationToken);

            var stats = new
            {
                PublishedCount = allUserPosts.Count(p => p.IsPublished),
                DraftCount = allUserPosts.Count(p => !p.IsPublished),
                FeaturedCount = allUserPosts.Count(p => p.IsFeatured),
                TotalViews = allUserPosts.Sum(p => p.ViewCount),
                TotalLikes = allUserPosts.Sum(p => p.LikeCount),
                TotalComments = allUserPosts.Sum(p => p.CommentCount)
            };

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(p => EF.Property<object>(p, request.SortBy))
                : query.OrderByDescending(p => EF.Property<object>(p, request.SortBy));

            // Apply pagination
            var posts = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new UserPostItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Excerpt = p.Excerpt,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    IsPublished = p.IsPublished,
                    IsFeatured = p.IsFeatured,
                    CreatedAt = p.CreatedAt,
                    PublishedAt = p.PublishedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CommunityId = p.CommunityId,
                    CommunityName = p.Community != null ? p.Community.Name : null,
                    ViewCount = p.ViewCount,
                    LikeCount = p.LikeCount,
                    CommentCount = p.CommentCount,
                    Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
                })
                .ToListAsync(cancellationToken);

            return new GetUserPostsResult
            {
                Posts = posts,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                PublishedCount = stats.PublishedCount,
                DraftCount = stats.DraftCount,
                FeaturedCount = stats.FeaturedCount,
                TotalViews = stats.TotalViews,
                TotalLikes = stats.TotalLikes,
                TotalComments = stats.TotalComments
            };
        }
    }
}



