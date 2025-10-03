using AutoMapper;
using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogHybrid.Application.Handlers.Community
{
    public class UpdateCommunityHandler : IRequestHandler<UpdateCommunityCommand, UpdateCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOptions<CommunitySettings> _communitySettings;
        private readonly ILogger<UpdateCommunityHandler> _logger;

        public UpdateCommunityHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<CommunitySettings> communitySettings,
            ILogger<UpdateCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _communitySettings = communitySettings;
            _logger = logger;
        }

        public async Task<UpdateCommunityResult> Handle(UpdateCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new UpdateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new UpdateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community name is required" }
                    };
                }

                var settings = _communitySettings.Value;

                // Check name length
                if (request.Name.Length < settings.MinNameLength || request.Name.Length > settings.MaxNameLength)
                {
                    return new UpdateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { $"Community name must be between {settings.MinNameLength} and {settings.MaxNameLength} characters" }
                    };
                }

                // Get existing community
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.Id, cancellationToken);
                if (community == null)
                {
                    return new UpdateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check permission (only creator or admin can update)
                var isCreator = community.CreatorId == request.CurrentUserId;
                var isAdmin = await _unitOfWork.Communities.IsModeratorOrAdminAsync(community.Id, request.CurrentUserId, cancellationToken);

                if (!isCreator && !isAdmin)
                {
                    return new UpdateCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to update this community" }
                    };
                }

                // Check if new category exists (if changing)
                if (request.CategoryId != community.CategoryId)
                {
                    var newCategory = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
                    if (newCategory == null)
                    {
                        return new UpdateCommunityResult
                        {
                            Success = false,
                            Errors = new List<string> { "New category not found" }
                        };
                    }
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Generate new slug if name changed
                var slug = community.Slug;
                if (community.Name != request.Name.Trim())
                {
                    slug = await SlugGenerator.GenerateUniqueSlug(
                        request.Name,
                        async (slugToCheck, excludeId) => await _unitOfWork.Communities
                            .SlugExistsAsync(slugToCheck, excludeId, cancellationToken),
                        community.Id
                    );
                }

                // Update community properties
                community.Name = request.Name.Trim();
                community.Slug = slug;
                community.Description = request.Description?.Trim() ?? string.Empty;
                community.ImageUrl = request.ImageUrl?.Trim();
                community.CoverImageUrl = request.CoverImageUrl?.Trim();
                community.Rules = request.Rules?.Trim();
                community.CategoryId = request.CategoryId;
                community.IsPrivate = request.IsPrivate;
                community.RequireApproval = request.RequireApproval;
                community.IsActive = request.IsActive;
                community.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Get updated category info for FullSlug
                var category = await _unitOfWork.Categories.GetByIdAsync(community.CategoryId, cancellationToken);

                _logger.LogInformation("Community updated: {CommunityId} - {CommunityName} by {UserId}",
                    community.Id, community.Name, request.CurrentUserId);

                return new UpdateCommunityResult
                {
                    Success = true,
                    Slug = community.Slug,
                    FullSlug = $"{category?.Slug}/{community.Slug}",
                    Message = "Community updated successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error updating community: {CommunityId}", request.Id);

                return new UpdateCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating the community" }
                };
            }
        }
    }
}