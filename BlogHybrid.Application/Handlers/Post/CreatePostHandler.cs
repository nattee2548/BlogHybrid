using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BlogHybrid.Application.Handlers.Post
{
    public class CreatePostHandler : IRequestHandler<CreatePostCommand, CreatePostResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreatePostHandler> _logger;

        public CreatePostHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreatePostHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ===================================
                // ✅ Validation 1: User must be logged in
                // ===================================
                if (string.IsNullOrWhiteSpace(request.AuthorId))
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "กรุณาเข้าสู่ระบบก่อนสร้างโพสต์" }
                    };
                }

                // ===================================
                // ✅ Validation 2: ต้องเลือกอย่างน้อย 1 อย่าง (Category หรือ Community)
                // ===================================
                if (!request.CategoryId.HasValue && !request.CommunityId.HasValue)
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "กรุณาเลือกหมวดหมู่ หรือ ชุมชน" }
                    };
                }

                // ===================================
                // ✅ Validation 3: Title is required
                // ===================================
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "กรุณากรอกหัวข้อโพสต์" }
                    };
                }

                if (request.Title.Length < 3 || request.Title.Length > 200)
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "หัวข้อโพสต์ต้องมีความยาว 3-200 ตัวอักษร" }
                    };
                }

                // ===================================
                // ✅ Validation 4: Content is required
                // ===================================
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "กรุณากรอกเนื้อหาโพสต์" }
                    };
                }

                if (request.Content.Length < 10)
                {
                    return new CreatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "เนื้อหาโพสต์ต้องมีความยาวอย่างน้อย 10 ตัวอักษร" }
                    };
                }

                // ===================================
                // ✅ Validation 5: ถ้าเลือก Category ต้องมีอยู่จริง
                // ===================================
                if (request.CategoryId.HasValue)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value, cancellationToken);

                    if (category == null)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "ไม่พบหมวดหมู่ที่เลือก" }
                        };
                    }

                    if (!category.IsActive)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "หมวดหมู่นี้ไม่เปิดใช้งาน" }
                        };
                    }
                }

                // ===================================
                // ✅ Validation 6: ถ้าเลือก Community ต้องตรวจสอบสิทธิ์
                // ===================================
                if (request.CommunityId.HasValue)
                {
                    var community = await _unitOfWork.Communities.GetByIdAsync(request.CommunityId.Value, cancellationToken);

                    if (community == null)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "ไม่พบชุมชนที่เลือก" }
                        };
                    }

                    if (!community.IsActive)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "ชุมชนนี้ไม่เปิดใช้งาน" }
                        };
                    }

                    // ตรวจสอบว่าเป็นสมาชิกหรือไม่
                    var membership = await _unitOfWork.Communities.GetMemberAsync(
                        community.Id,
                        request.AuthorId!,
                        cancellationToken
                    );

                    if (membership == null || !membership.IsApproved)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "คุณไม่มีสิทธิ์โพสต์ในชุมชนนี้ (ต้องเป็นสมาชิกก่อน)" }
                        };
                    }

                    // ถ้าถูก Ban ห้ามโพสต์
                    if (membership.IsBanned)
                    {
                        return new CreatePostResult
                        {
                            Success = false,
                            Errors = new List<string> { "คุณถูกแบนจากชุมชนนี้" }
                        };
                    }
                }

                // ===================================
                // ✅ Generate Slug
                // ===================================
                var slug = GenerateSlug(request.Title);

                // ตรวจสอบว่า slug ซ้ำหรือไม่
                var slugExists = await _unitOfWork.DbContext.Set<Domain.Entities.Post>()
                    .AnyAsync(p => p.Slug == slug, cancellationToken);

                if (slugExists)
                {
                    // เพิ่ม timestamp ถ้า slug ซ้ำ
                    slug = $"{slug}-{DateTime.UtcNow.Ticks}";
                }

                // ===================================
                // ✅ Auto-generate Excerpt if not provided
                // ===================================
                var excerpt = request.Excerpt;
                if (string.IsNullOrWhiteSpace(excerpt))
                {
                    excerpt = GenerateExcerpt(request.Content);
                }

                // ===================================
                // ✅ Create Post
                // ===================================
                var post = new Domain.Entities.Post
                {
                    Title = request.Title.Trim(),
                    Slug = slug,
                    Content = request.Content.Trim(),
                    Excerpt = excerpt,
                    FeaturedImageUrl = request.FeaturedImageUrl,
                    AuthorId = request.AuthorId!,
                    CategoryId = request.CategoryId,
                    CommunityId = request.CommunityId,
                    IsPublished = request.IsPublished,
                    IsFeatured = request.IsFeatured,
                    PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add to database
                await _unitOfWork.DbContext.Set<Domain.Entities.Post>().AddAsync(post, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // ===================================
                // ✅ Process Tags (if provided)
                // ===================================
                if (!string.IsNullOrWhiteSpace(request.Tags))
                {
                    await ProcessTags(post.Id, request.Tags, cancellationToken);
                }

                // ===================================
                // ✅ Increment Post Count (if in Community)
                // ===================================
                if (request.CommunityId.HasValue)
                {
                    await _unitOfWork.Communities.IncrementPostCountAsync(
                        request.CommunityId.Value,
                        cancellationToken
                    );
                }

                _logger.LogInformation(
                    "Post created successfully. PostId: {PostId}, Title: {Title}, Author: {AuthorId}",
                    post.Id, post.Title, request.AuthorId
                );

                return new CreatePostResult
                {
                    Success = true,
                    PostId = post.Id,
                    Slug = post.Slug,
                    Message = "สร้างโพสต์สำเร็จ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post. Title: {Title}", request.Title);
                return new CreatePostResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการสร้างโพสต์" }
                };
            }
        }

        // ===================================
        // Helper Methods
        // ===================================

        private string GenerateSlug(string title)
        {
            // Remove special characters and convert to lowercase
            var slug = title.ToLowerInvariant();

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove all non-alphanumeric characters except hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-ก-๙]", "");

            // Remove duplicate hyphens
            slug = Regex.Replace(slug, @"\-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            // Limit length
            if (slug.Length > 200)
            {
                slug = slug.Substring(0, 200).TrimEnd('-');
            }

            return slug;
        }

        private string GenerateExcerpt(string content)
        {
            // Remove HTML tags
            var plainText = Regex.Replace(content, "<.*?>", string.Empty);

            // Limit to 300 characters
            if (plainText.Length > 300)
            {
                plainText = plainText.Substring(0, 300) + "...";
            }

            return plainText.Trim();
        }

        private async Task ProcessTags(int postId, string tagsString, CancellationToken cancellationToken)
        {
            try
            {
                // Parse tags from comma-separated string
                var tagNames = tagsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct()
                    .Take(10) // จำกัดไม่เกิน 10 tags
                    .ToList();

                foreach (var tagName in tagNames)
                {
                    // Find or create tag
                    var tagSlug = GenerateSlug(tagName);
                    var tag = await _unitOfWork.Tags.GetBySlugAsync(tagSlug, cancellationToken);

                    if (tag == null)
                    {
                        // Create new tag
                        tag = new Domain.Entities.Tag
                        {
                            Name = tagName,
                            Slug = tagSlug,
                            CreatedAt = DateTime.UtcNow
                        };
                        tag = await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // Create PostTag relationship
                    var postTag = new Domain.Entities.PostTag
                    {
                        PostId = postId,
                        TagId = tag.Id
                    };

                    await _unitOfWork.DbContext.Set<Domain.Entities.PostTag>().AddAsync(postTag, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tags for post {PostId}", postId);
                // Don't throw - tags are not critical
            }
        }
    }
}