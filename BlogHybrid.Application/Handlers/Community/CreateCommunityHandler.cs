using AutoMapper;
using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Enums;
using BlogHybrid.Application.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogHybrid.Application.Handlers.Community
{
    public class CreateCommunityHandler : IRequestHandler<CreateCommunityCommand, CreateCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOptions<CommunitySettings> _communitySettings;
        private readonly ILogger<CreateCommunityHandler> _logger;

        public CreateCommunityHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<CommunitySettings> communitySettings,
            ILogger<CreateCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _communitySettings = communitySettings;
            _logger = logger;
        }

        public async Task<CreateCommunityResult> Handle(CreateCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.CreatorId))
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in to create a community" }
                    };
                }

                // ✅ Parse CategoryIds from comma-separated string
                var categoryIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(request.CategoryIds))
                {
                    try
                    {
                        categoryIds = request.CategoryIds
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => int.Parse(id.Trim()))
                            .ToList();
                    }
                    catch
                    {
                        return new CreateCommunityResult
                        {
                            Success = false,
                            Errors = new List<string> { "Invalid category IDs format" }
                        };
                    }
                }

                // ✅ Validate: ต้องเลือกอย่างน้อย 1 category
                if (categoryIds.Count == 0)
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Please select at least one category" }
                    };
                }

                // ✅ Validate: ไม่เกินจำนวนที่กำหนด (config)
                var maxCategories = _communitySettings.Value.MaxCategoriesPerCommunity;
                if (categoryIds.Count > maxCategories)
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { $"You can select maximum {maxCategories} categories" }
                    };
                }

                // ✅ Validate: ตรวจสอบว่า categories ทั้งหมดมีอยู่จริง
                var categories = new List<BlogHybrid.Domain.Entities.Category>();
                foreach (var catId in categoryIds)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(catId, cancellationToken);
                    if (category == null)
                    {
                        return new CreateCommunityResult
                        {
                            Success = false,
                            Errors = new List<string> { $"Category ID {catId} not found" }
                        };
                    }
                    categories.Add(category);
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Generate unique slug
                var slug = await SlugGenerator.GenerateUniqueSlug(
                    request.Name,
                    async (slugToCheck, excludeId) => await _unitOfWork.Communities
                        .SlugExistsAsync(slugToCheck, excludeId, cancellationToken)
                );

                // Create community
                var community = _mapper.Map<BlogHybrid.Domain.Entities.Community>(request);
                community.Slug = slug;
                community.CreatorId = request.CreatorId;
                community.CreatedAt = DateTime.UtcNow;
                community.UpdatedAt = DateTime.UtcNow;
                community.MemberCount = 1; // Creator is the first member
                community.PostCount = 0;
                community.IsActive = true;
                community.IsDeleted = false;

                await _unitOfWork.Communities.AddAsync(community, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // ✅ เพิ่ม Categories ลงตาราง CommunityCategory
                foreach (var categoryId in categoryIds)
                {
                    var communityCategory = new BlogHybrid.Domain.Entities.CommunityCategory
                    {
                        CommunityId = community.Id,
                        CategoryId = categoryId,
                        AssignedAt = DateTime.UtcNow
                    };

                    // ใช้ DbContext.Set<CommunityCategory>().Add() หรือ Repository (ถ้ามี)
                    // สมมุติว่าใช้ DbContext โดยตรง:
                    await _unitOfWork.DbContext.Set<BlogHybrid.Domain.Entities.CommunityCategory>()
                        .AddAsync(communityCategory, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Add creator as Admin member
                var creatorMember = new BlogHybrid.Domain.Entities.CommunityMember
                {
                    CommunityId = community.Id,
                    UserId = request.CreatorId,
                    Role = CommunityRole.Admin,
                    JoinedAt = DateTime.UtcNow,
                    IsApproved = true,
                    IsBanned = false
                };

                await _unitOfWork.Communities.AddMemberAsync(creatorMember, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // ✅ ใช้ Category แรกสำหรับ FullSlug
                var firstCategory = categories.First();

                _logger.LogInformation("Community created: {CommunityId} - {CommunityName} by {CreatorId} with {CategoryCount} categories",
                    community.Id, community.Name, request.CreatorId, categoryIds.Count);

                return new CreateCommunityResult
                {
                    Success = true,
                    CommunityId = community.Id,
                    Slug = community.Slug,
                    FullSlug = $"{firstCategory.Slug}/{community.Slug}",
                    Message = "Community created successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error creating community: {CommunityName}", request.Name);

                return new CreateCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while creating the community" }
                };
            }
        }
    }
}