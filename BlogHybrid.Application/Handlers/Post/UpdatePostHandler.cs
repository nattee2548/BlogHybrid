using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Post
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, UpdatePostResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePostCommandHandler> _logger;

        public UpdatePostCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdatePostCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UpdatePostResult> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ FIX: ใช้ GetByIdWithDetailsAsync เพื่อโหลด PostTags
                var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(request.Id);

                if (post == null)
                {
                    return new UpdatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบโพสต์ที่ต้องการแก้ไข" }
                    };
                }

                // ตรวจสอบสิทธิ์
                if (post.AuthorId != request.AuthorId)
                {
                    return new UpdatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "คุณไม่มีสิทธิ์แก้ไขโพสต์นี้" }
                    };
                }

                // อัปเดตข้อมูลพื้นฐาน
                post.Title = request.Title;
                post.Content = request.Content;
                post.Excerpt = request.Excerpt;
                post.FeaturedImageUrl = request.FeaturedImageUrl;
                post.CategoryId = request.CategoryId;
                post.CommunityId = request.CommunityId;
                post.IsPublished = request.IsPublished;
                post.IsFeatured = request.IsFeatured;
                post.UpdatedAt = DateTime.UtcNow;

                // อัปเดต Slug ถ้า Title เปลี่ยน
                var newSlug = GenerateSlug(request.Title);
                if (post.Slug != newSlug)
                {
                    var allPosts = await _unitOfWork.Posts.GetAllAsync();
                    var existingPost = allPosts.FirstOrDefault(p => p.Slug == newSlug && p.Id != request.Id);

                    if (existingPost != null)
                    {
                        newSlug = $"{newSlug}-{DateTime.UtcNow.Ticks}";
                    }

                    post.Slug = newSlug;
                }

                // ✅ FIX: ลบและเพิ่ม Tags ใหม่ทั้งหมด
                // ลบ PostTags เดิมทั้งหมดด้วย DbContext
                var existingPostTags = await _unitOfWork.DbContext.Set<PostTag>()
                    .Where(pt => pt.PostId == post.Id)
                    .ToListAsync(cancellationToken);

                if (existingPostTags.Any())
                {
                    _unitOfWork.DbContext.Set<PostTag>().RemoveRange(existingPostTags);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // เพิ่ม Tags ใหม่
                if (!string.IsNullOrWhiteSpace(request.Tags))
                {
                    var tagNames = request.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()
                        .Take(10)
                        .ToList();

                    foreach (var tagName in tagNames)
                    {
                        // หา Tag ที่มีอยู่แล้ว
                        var allTags = await _unitOfWork.Tags.GetAllAsync();
                        var tag = allTags.FirstOrDefault(t =>
                            t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));

                        // ถ้าไม่มี ให้สร้างใหม่
                        if (tag == null)
                        {
                            tag = new Domain.Entities.Tag
                            {
                                Name = tagName,
                                Slug = GenerateSlug(tagName),
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Tags.AddAsync(tag);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }

                        // เพิ่ม PostTag ใหม่
                        var postTag = new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        };
                        await _unitOfWork.DbContext.Set<PostTag>().AddAsync(postTag, cancellationToken);
                    }
                }

                // บันทึกการเปลี่ยนแปลง
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Post updated successfully: ID={post.Id}, Title={post.Title}");

                return new UpdatePostResult
                {
                    Success = true,
                    Slug = post.Slug,
                    Message = "อัปเดตโพสต์สำเร็จ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post ID: {request.Id}");
                return new UpdatePostResult
                {
                    Success = false,
                    Errors = new List<string> { $"เกิดข้อผิดพลาด: {ex.Message}" }
                };
            }
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            var slug = title.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9ก-๙\-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\-{2,}", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}