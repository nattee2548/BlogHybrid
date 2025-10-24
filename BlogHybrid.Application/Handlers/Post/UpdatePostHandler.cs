// ===================================================
// UpdatePostCommandHandler.cs - FIXED VERSION
// Location: BlogHybrid.Application/Handlers/Post/
// ===================================================

using BlogHybrid.Application.Commands.Post;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;  // ⭐ เพิ่ม using นี้
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
                // ดึงโพสต์ที่ต้องการแก้ไข
                var post = await _unitOfWork.Posts.GetByIdAsync(request.Id);

                if (post == null)
                {
                    return new UpdatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบโพสต์ที่ต้องการแก้ไข" }
                    };
                }

                // ตรวจสอบว่าเป็นเจ้าของโพสต์หรือไม่
                if (post.AuthorId != request.AuthorId)
                {
                    return new UpdatePostResult
                    {
                        Success = false,
                        Errors = new List<string> { "คุณไม่มีสิทธิ์แก้ไขโพสต์นี้" }
                    };
                }

                // อัปเดตข้อมูล
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
                    // ตรวจสอบว่า Slug ซ้ำหรือไม่
                    var existingPosts = await _unitOfWork.Posts.GetAllAsync();
                    var existingPost = existingPosts.FirstOrDefault(p => p.Slug == newSlug && p.Id != request.Id);

                    if (existingPost != null)
                    {
                        // เพิ่มเลขท้าย
                        newSlug = $"{newSlug}-{DateTime.UtcNow.Ticks}";
                    }

                    post.Slug = newSlug;
                }

                // ⭐ อัปเดต Tags - ใช้วิธีง่ายๆ โดยลบและสร้างใหม่
                if (!string.IsNullOrWhiteSpace(request.Tags))
                {
                    // ลบ Tags เดิมทั้งหมด (จะทำผ่าน Navigation Property)
                    post.PostTags.Clear();

                    // เพิ่ม Tags ใหม่
                    var tagNames = request.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()
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
                            tag = new Domain.Entities.Tag  // ⭐ ใช้ Full namespace
                            {
                                Name = tagName,
                                Slug = GenerateSlug(tagName),
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Tags.AddAsync(tag);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }

                        // เพิ่ม PostTag
                        post.PostTags.Add(new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });
                    }
                }
                else
                {
                    // ถ้าไม่มี Tags ใหม่ ให้ลบทั้งหมด
                    post.PostTags.Clear();
                }

                // บันทึก - ไม่ต้อง Update อีกรอบ เพราะ EF Core tracking อยู่แล้ว
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
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการอัปเดตโพสต์" }
                };
            }
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // แปลงเป็นตัวพิมพ์เล็ก
            var slug = title.ToLowerInvariant();

            // แทนที่ช่องว่างด้วย -
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

            // ลบตัวอักษรพิเศษ
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9ก-๙\-]", "");

            // ลบ - ที่ซ้ำกัน
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\-{2,}", "-");

            // ลบ - ที่ขึ้นต้นและลงท้าย
            slug = slug.Trim('-');

            return slug;
        }
    }
}