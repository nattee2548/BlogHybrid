using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Configuration;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Delete Community Handler

    public class DeleteCommunityHandler : IRequestHandler<DeleteCommunityCommand, DeleteCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<CommunitySettings> _communitySettings;
        private readonly ILogger<DeleteCommunityHandler> _logger;

        public DeleteCommunityHandler(
            IUnitOfWork unitOfWork,
            IOptions<CommunitySettings> communitySettings,
            ILogger<DeleteCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _communitySettings = communitySettings;
            _logger = logger;
        }

        public async Task<DeleteCommunityResult> Handle(DeleteCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new DeleteCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.Id, cancellationToken);
                if (community == null)
                {
                    return new DeleteCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check permission (only creator or system admin)
                var isCreator = community.CreatorId == request.CurrentUserId;
                // TODO: Add system admin check when we have admin role checking
                var isSystemAdmin = false; // await CheckSystemAdminRole(request.CurrentUserId);

                if (!isCreator && !isSystemAdmin)
                {
                    return new DeleteCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to delete this community" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                if (request.PermanentDelete && isSystemAdmin)
                {
                    // Permanent delete (admin only)
                    await _unitOfWork.Communities.DeleteAsync(community, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation("Community permanently deleted: {CommunityId} - {CommunityName} by {UserId}",
                        community.Id, community.Name, request.CurrentUserId);

                    return new DeleteCommunityResult
                    {
                        Success = true,
                        Message = "Community permanently deleted",
                        IsSoftDeleted = false
                    };
                }
                else
                {
                    // Soft delete
                    await _unitOfWork.Communities.SoftDeleteAsync(community.Id, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    var retentionDays = _communitySettings.Value.SoftDeleteRetentionDays;
                    var canRestoreUntil = DateTime.UtcNow.AddDays(retentionDays);

                    _logger.LogInformation("Community soft deleted: {CommunityId} - {CommunityName} by {UserId}",
                        community.Id, community.Name, request.CurrentUserId);

                    return new DeleteCommunityResult
                    {
                        Success = true,
                        Message = $"Community deleted. You can restore it within {retentionDays} days",
                        IsSoftDeleted = true,
                        CanRestoreUntil = canRestoreUntil
                    };
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting community: {CommunityId}", request.Id);

                return new DeleteCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while deleting the community" }
                };
            }
        }
    }

    #endregion

    #region Restore Community Handler

    public class RestoreCommunityHandler : IRequestHandler<RestoreCommunityCommand, RestoreCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<CommunitySettings> _communitySettings;
        private readonly ILogger<RestoreCommunityHandler> _logger;

        public RestoreCommunityHandler(
            IUnitOfWork unitOfWork,
            IOptions<CommunitySettings> communitySettings,
            ILogger<RestoreCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _communitySettings = communitySettings;
            _logger = logger;
        }

        public async Task<RestoreCommunityResult> Handle(RestoreCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new RestoreCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get with IgnoreQueryFilters to see deleted communities
                var community = await _unitOfWork.Communities.GetByIdAsync(request.Id, cancellationToken);
                if (community == null)
                {
                    return new RestoreCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                if (!community.IsDeleted)
                {
                    return new RestoreCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community is not deleted" }
                    };
                }

                // Check permission
                var isCreator = community.CreatorId == request.CurrentUserId;
                var isSystemAdmin = false; // TODO: Add system admin check

                if (!isCreator && !isSystemAdmin)
                {
                    return new RestoreCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to restore this community" }
                    };
                }

                // Check if still within retention period
                var retentionDays = _communitySettings.Value.SoftDeleteRetentionDays;
                var canRestoreUntil = community.DeletedAt!.Value.AddDays(retentionDays);

                if (DateTime.UtcNow > canRestoreUntil)
                {
                    return new RestoreCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { $"Restore period expired. Community was deleted more than {retentionDays} days ago" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                await _unitOfWork.Communities.RestoreAsync(community.Id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Community restored: {CommunityId} - {CommunityName} by {UserId}",
                    community.Id, community.Name, request.CurrentUserId);

                return new RestoreCommunityResult
                {
                    Success = true,
                    Message = "Community restored successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error restoring community: {CommunityId}", request.Id);

                return new RestoreCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while restoring the community" }
                };
            }
        }
    }

    #endregion

    #region Toggle Community Status Handler

    public class ToggleCommunityStatusHandler : IRequestHandler<ToggleCommunityStatusCommand, ToggleCommunityStatusResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ToggleCommunityStatusHandler> _logger;

        public ToggleCommunityStatusHandler(
            IUnitOfWork unitOfWork,
            ILogger<ToggleCommunityStatusHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ToggleCommunityStatusResult> Handle(ToggleCommunityStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new ToggleCommunityStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                var community = await _unitOfWork.Communities.GetByIdAsync(request.Id, cancellationToken);
                if (community == null)
                {
                    return new ToggleCommunityStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check permission
                var isCreator = community.CreatorId == request.CurrentUserId;
                var isAdmin = await _unitOfWork.Communities.IsModeratorOrAdminAsync(community.Id, request.CurrentUserId, cancellationToken);

                if (!isCreator && !isAdmin)
                {
                    return new ToggleCommunityStatusResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to change community status" }
                    };
                }

                community.IsActive = request.IsActive;
                community.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var statusText = request.IsActive ? "activated" : "deactivated";
                _logger.LogInformation("Community {Status}: {CommunityId} - {CommunityName} by {UserId}",
                    statusText, community.Id, community.Name, request.CurrentUserId);

                return new ToggleCommunityStatusResult
                {
                    Success = true,
                    NewStatus = community.IsActive,
                    Message = $"Community {statusText} successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling community status: {CommunityId}", request.Id);

                return new ToggleCommunityStatusResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while changing community status" }
                };
            }
        }
    }

    #endregion
}