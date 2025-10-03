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

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community name is required" }
                    };
                }

                var settings = _communitySettings.Value;

                // Check name length
                if (request.Name.Length < settings.MinNameLength || request.Name.Length > settings.MaxNameLength)
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { $"Community name must be between {settings.MinNameLength} and {settings.MaxNameLength} characters" }
                    };
                }

                // Check user community limit (ไม่นับ soft deleted)
                var userCommunityCount = await _unitOfWork.Communities
                    .GetUserCommunityCountAsync(request.CreatorId, includeDeleted: false, cancellationToken);

                if (userCommunityCount >= settings.MaxCommunitiesPerUser)
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { $"You can only create up to {settings.MaxCommunitiesPerUser} communities" }
                    };
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
                if (category == null)
                {
                    return new CreateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Category not found" }
                    };
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

                _logger.LogInformation("Community created: {CommunityId} - {CommunityName} by {CreatorId}",
                    community.Id, community.Name, request.CreatorId);

                return new CreateCommunityResult
                {
                    Success = true,
                    CommunityId = community.Id,
                    Slug = community.Slug,
                    FullSlug = $"{category.Slug}/{community.Slug}",
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