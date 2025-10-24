using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Post
{
    public class GetPostForEditHandler : IRequestHandler<GetPostForEditQuery, PostForEditResult?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPostForEditHandler> _logger;

        public GetPostForEditHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPostForEditHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PostForEditResult?> Handle(
            GetPostForEditQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var post = await _unitOfWork.Posts
                    .GetQueryable()
                    .Include(p => p.Category)
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.Id == request.PostId && !p.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("Post not found. PostId: {PostId}", request.PostId);
                    return null;
                }

                // ✅ Authorization: เฉพาะเจ้าของโพสต์เท่านั้นที่แก้ไขได้
                if (post.AuthorId != request.CurrentUserId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to edit post {PostId} owned by {OwnerId}",
                        request.CurrentUserId, post.Id, post.AuthorId
                    );
                    return null;
                }

                // Map to result
                var result = new PostForEditResult
                {
                    Id = post.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Content = post.Content,
                    Excerpt = post.Excerpt,
                    FeaturedImageUrl = post.FeaturedImageUrl,
                    CategoryId = post.CategoryId,
                    CategoryName = post.Category?.Name,
                    CommunityId = post.CommunityId,
                    CommunityName = post.Community?.Name,
                    Tags = string.Join(", ", post.PostTags.Select(pt => pt.Tag.Name)),
                    IsPublished = post.IsPublished,
                    IsFeatured = post.IsFeatured,
                    AuthorId = post.AuthorId,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for edit. PostId: {PostId}", request.PostId);
                return null;
            }
        }
    }
}