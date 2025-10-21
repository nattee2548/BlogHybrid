using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Approve Member Handler

    public class ApproveMemberHandler : IRequestHandler<ApproveMemberCommand, ApproveMemberResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApproveMemberHandler> _logger;

        public ApproveMemberHandler(
            IUnitOfWork unitOfWork,
            ILogger<ApproveMemberHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApproveMemberResult> Handle(ApproveMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check if current user is Admin or Moderator
                var isAuthorized = await _unitOfWork.Communities.IsModeratorOrAdminAsync(
                    request.CommunityId,
                    request.CurrentUserId,
                    cancellationToken);

                if (!isAuthorized)
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to approve members" }
                    };
                }

                // Get member
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.MemberUserId,
                    cancellationToken);

                if (member == null)
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member not found" }
                    };
                }

                if (member.IsApproved)
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member is already approved" }
                    };
                }

                if (member.IsBanned)
                {
                    return new ApproveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot approve banned member" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Approve member
                member.IsApproved = true;
                await _unitOfWork.Communities.UpdateMemberAsync(member, cancellationToken);

                // Update member count
                community.MemberCount++;
                await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Member approved: {MemberUserId} in community {CommunityId} by {CurrentUserId}",
                    request.MemberUserId, request.CommunityId, request.CurrentUserId);

                return new ApproveMemberResult
                {
                    Success = true,
                    Message = "Member approved successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error approving member {MemberUserId} in community {CommunityId}",
                    request.MemberUserId, request.CommunityId);

                return new ApproveMemberResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while approving member" }
                };
            }
        }
    }

    #endregion

    #region Reject Member Handler

    public class RejectMemberHandler : IRequestHandler<RejectMemberCommand, RejectMemberResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RejectMemberHandler> _logger;

        public RejectMemberHandler(
            IUnitOfWork unitOfWork,
            ILogger<RejectMemberHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RejectMemberResult> Handle(RejectMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new RejectMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new RejectMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Check if current user is Admin or Moderator
                var isAuthorized = await _unitOfWork.Communities.IsModeratorOrAdminAsync(
                    request.CommunityId,
                    request.CurrentUserId,
                    cancellationToken);

                if (!isAuthorized)
                {
                    return new RejectMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to reject members" }
                    };
                }

                // Get member
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.MemberUserId,
                    cancellationToken);

                if (member == null)
                {
                    return new RejectMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member not found" }
                    };
                }

                if (member.IsApproved)
                {
                    return new RejectMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot reject approved member. Use remove instead." }
                    };
                }

                // Remove member (reject)
                await _unitOfWork.Communities.RemoveMemberAsync(request.CommunityId, request.MemberUserId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Member rejected: {MemberUserId} in community {CommunityId} by {CurrentUserId}",
                    request.MemberUserId, request.CommunityId, request.CurrentUserId);

                return new RejectMemberResult
                {
                    Success = true,
                    Message = "Member join request rejected"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting member {MemberUserId} in community {CommunityId}",
                    request.MemberUserId, request.CommunityId);

                return new RejectMemberResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while rejecting member" }
                };
            }
        }
    }

    #endregion
}