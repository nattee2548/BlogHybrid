using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Join Community Handler

    public class JoinCommunityHandler : IRequestHandler<JoinCommunityCommand, JoinCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<JoinCommunityHandler> _logger;

        public JoinCommunityHandler(
            IUnitOfWork unitOfWork,
            ILogger<JoinCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<JoinCommunityResult> Handle(JoinCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return new JoinCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new JoinCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check if community is active
                if (!community.IsActive)
                {
                    return new JoinCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "This community is not active" }
                    };
                }

                // Check if already a member
                var existingMember = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.UserId,
                    cancellationToken);

                if (existingMember != null)
                {
                    if (existingMember.IsBanned)
                    {
                        return new JoinCommunityResult
                        {
                            Success = false,
                            Errors = new List<string> { "You are banned from this community" }
                        };
                    }

                    if (existingMember.IsApproved)
                    {
                        return new JoinCommunityResult
                        {
                            Success = false,
                            Errors = new List<string> { "You are already a member of this community" }
                        };
                    }

                    // Already have pending request
                    return new JoinCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "You already have a pending join request" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Create new member
                var newMember = new CommunityMember
                {
                    CommunityId = request.CommunityId,
                    UserId = request.UserId,
                    Role = CommunityRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsApproved = !community.RequireApproval, // Auto-approve if not required
                    IsBanned = false
                };

                await _unitOfWork.Communities.AddMemberAsync(newMember, cancellationToken);

                // Update member count if approved
                if (newMember.IsApproved)
                {
                    community.MemberCount++;
                    await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("User {UserId} joined community {CommunityId} - RequiresApproval: {RequiresApproval}",
                    request.UserId, request.CommunityId, community.RequireApproval);

                return new JoinCommunityResult
                {
                    Success = true,
                    RequiresApproval = community.RequireApproval,
                    Message = community.RequireApproval
                        ? "Join request submitted. Waiting for approval."
                        : "Successfully joined the community!"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error joining community {CommunityId} for user {UserId}",
                    request.CommunityId, request.UserId);

                return new JoinCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while joining the community" }
                };
            }
        }
    }

    #endregion

    #region Leave Community Handler

    public class LeaveCommunityHandler : IRequestHandler<LeaveCommunityCommand, LeaveCommunityResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveCommunityHandler> _logger;

        public LeaveCommunityHandler(
            IUnitOfWork unitOfWork,
            ILogger<LeaveCommunityHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LeaveCommunityResult> Handle(LeaveCommunityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return new LeaveCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new LeaveCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check if user is the creator
                if (community.CreatorId == request.UserId)
                {
                    return new LeaveCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community creator cannot leave the community. Please delete the community instead." }
                    };
                }

                // Check if member exists
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.UserId,
                    cancellationToken);

                if (member == null)
                {
                    return new LeaveCommunityResult
                    {
                        Success = false,
                        Errors = new List<string> { "You are not a member of this community" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Remove member
                await _unitOfWork.Communities.RemoveMemberAsync(request.CommunityId, request.UserId, cancellationToken);

                // Update member count if was approved
                if (member.IsApproved)
                {
                    community.MemberCount = Math.Max(0, community.MemberCount - 1);
                    await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("User {UserId} left community {CommunityId}",
                    request.UserId, request.CommunityId);

                return new LeaveCommunityResult
                {
                    Success = true,
                    Message = "Successfully left the community"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error leaving community {CommunityId} for user {UserId}",
                    request.CommunityId, request.UserId);

                return new LeaveCommunityResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while leaving the community" }
                };
            }
        }
    }

    #endregion
}