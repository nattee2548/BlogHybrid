// BlogHybrid.Application/Handlers/Post/GetPostDetailBySlugHandler.cs
using BlogHybrid.Application.DTOs.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Post
{
    public class GetPostDetailBySlugHandler : IRequestHandler<GetPostDetailBySlugQuery, PostDetailDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPostDetailBySlugHandler> _logger;

        public GetPostDetailBySlugHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPostDetailBySlugHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PostDetailDto?> Handle(
            GetPostDetailBySlugQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // ดึงโพสต์พร้อมข้อมูลที่เกี่ยวข้อง
                var post = await _unitOfWork.DbContext.Set<Domain.Entities.Post>()
                    .Include(p => p.Author)
                    .Include(p => p.Category)
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    // Include Comments ทั้งหมดที่ IsApproved พร้อม Author และ CommentLikes
                    .Include(p => p.Comments.Where(c => c.IsApproved))
                        .ThenInclude(c => c.Author)
                    .Include(p => p.Comments.Where(c => c.IsApproved))
                        .ThenInclude(c => c.CommentLikes)
                    .Include(p => p.PostLikes)
                    .Where(p => p.Slug == request.Slug && p.IsPublished && !p.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("Post not found: {Slug}", request.Slug);
                    return null;
                }

                // เพิ่ม view count (ในกรณีจริงควรทำใน background job)
                post.ViewCount++;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // เช็คว่า current user like โพสต์นี้หรือไม่
                bool isLikedByCurrentUser = false;
                if (!string.IsNullOrEmpty(request.CurrentUserId))
                {
                    isLikedByCurrentUser = post.PostLikes.Any(pl => pl.UserId == request.CurrentUserId);
                }

                // เช็คสิทธิ์
                bool canEdit = !string.IsNullOrEmpty(request.CurrentUserId) &&
                               post.AuthorId == request.CurrentUserId;
                bool canDelete = canEdit; // เดี๋ยวเพิ่ม admin role check ได้

                // สร้าง DTO
                var postDetail = new PostDetailDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Content = post.Content,
                    Excerpt = post.Excerpt,
                    FeaturedImageUrl = post.FeaturedImageUrl,
                    ViewCount = post.ViewCount,
                    LikeCount = post.LikeCount,
                    CommentCount = post.Comments.Count(c => c.IsApproved),
                    IsLikedByCurrentUser = isLikedByCurrentUser,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    PublishedAt = post.PublishedAt,

                    // Author
                    AuthorId = post.AuthorId,
                    AuthorDisplayName = post.Author.DisplayName,
                    AuthorUserName = post.Author.UserName ?? string.Empty,
                    AuthorProfileImageUrl = post.Author.ProfileImageUrl,
                    AuthorBio = post.Author.Bio,

                    // Category
                    CategoryId = post.CategoryId,
                    CategoryName = post.Category?.Name,
                    CategorySlug = post.Category?.Slug,
                    CategoryColor = post.Category?.Color,

                    // Community
                    CommunityId = post.CommunityId,
                    CommunityName = post.Community?.Name,
                    CommunitySlug = post.Community?.Slug,
                    CommunityImageUrl = post.Community?.ImageUrl,

                    // Tags
                    Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),

                    // Permissions
                    CanEdit = canEdit,
                    CanDelete = canDelete,

                    // Comments (จัด hierarchical) - ใช้เฉพาะ comments ที่ IsApproved
                    Comments = BuildCommentTree(post.Comments.Where(c => c.IsApproved).ToList(), request.CurrentUserId)
                };

                _logger.LogInformation("Retrieved post detail: {Slug}", request.Slug);
                return postDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post detail by slug: {Slug}", request.Slug);
                return null;
            }
        }

        /// <summary>
        /// สร้าง hierarchical comment tree
        /// </summary>
        private List<CommentDto> BuildCommentTree(
            List<Domain.Entities.Comment> allComments,
            string? currentUserId)
        {
            // หา root comments (ไม่มี parent)
            var rootComments = allComments
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => MapToCommentDto(c, allComments, currentUserId))
                .ToList();

            return rootComments;
        }

        /// <summary>
        /// แปลง Comment entity เป็น DTO พร้อม replies
        /// </summary>
        private CommentDto MapToCommentDto(
            Domain.Entities.Comment comment,
            List<Domain.Entities.Comment> allComments,
            string? currentUserId)
        {
            // เช็คว่า current user like comment นี้หรือไม่
            bool isLikedByCurrentUser = false;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isLikedByCurrentUser = comment.CommentLikes.Any(cl => cl.UserId == currentUserId);
            }

            // เช็คสิทธิ์
            bool canEdit = !string.IsNullOrEmpty(currentUserId) &&
                           comment.AuthorId == currentUserId;
            bool canDelete = canEdit;

            var dto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                LikeCount = comment.LikeCount,
                IsApproved = comment.IsApproved,
                IsLikedByCurrentUser = isLikedByCurrentUser,

                // Author
                AuthorId = comment.AuthorId,
                AuthorDisplayName = comment.Author.DisplayName,
                AuthorUserName = comment.Author.UserName ?? string.Empty,
                AuthorProfileImageUrl = comment.Author.ProfileImageUrl,

                ParentCommentId = comment.ParentCommentId,

                // Permissions
                CanEdit = canEdit,
                CanDelete = canDelete,

                // Recursively get replies (เฉพาะ replies ที่มี parent เป็น comment นี้)
                Replies = allComments
                    .Where(c => c.ParentCommentId == comment.Id)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => MapToCommentDto(c, allComments, currentUserId))
                    .ToList()
            };

            return dto;
        }
    }
}