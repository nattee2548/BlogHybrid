using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Change Member Role Handler

    public class ChangeMemberRoleHandler : IRequestHandler<ChangeMemberRoleCommand, ChangeMemberRoleResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChangeMemberRoleHandler> _logger;

        public ChangeMemberRoleHandler(
            IUnitOfWork unitOfWork,
            ILogger<ChangeMemberRoleHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ChangeMemberRoleResult> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Community not found" }
                    };
                }

                // Only creator can change roles
                if (community.CreatorId != request.CurrentUserId)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Only community creator can change member roles" }
                    };
                }

                // Get member
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.MemberUserId,
                    cancellationToken);

                if (member == null)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member not found" }
                    };
                }

                if (!member.IsApproved)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot change role of unapproved member" }
                    };
                }

                if (member.IsBanned)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot change role of banned member" }
                    };
                }

                // Cannot change creator's role
                if (member.UserId == community.CreatorId)
                {
                    return new ChangeMemberRoleResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot change community creator's role" }
                    };
                }

                var oldRole = member.Role;
                member.Role = request.NewRole;

                await _unitOfWork.Communities.UpdateMemberAsync(member, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Member role changed: {MemberUserId} from {OldRole} to {NewRole} in community {CommunityId} by {CurrentUserId}",
                    request.MemberUserId, oldRole, request.NewRole, request.CommunityId, request.CurrentUserId);

                return new ChangeMemberRoleResult
                {
                    Success = true,
                    NewRole = request.NewRole,
                    Message = $"Member role changed to {request.NewRole}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing member role {MemberUserId} in community {CommunityId}",
                    request.MemberUserId, request.CommunityId);

                return new ChangeMemberRoleResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while changing member role" }
                };
            }
        }
    }

    #endregion

    #region Ban Member Handler

    public class BanMemberHandler : IRequestHandler<BanMemberCommand, BanMemberResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BanMemberHandler> _logger;

        public BanMemberHandler(
            IUnitOfWork unitOfWork,
            ILogger<BanMemberHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BanMemberResult> Handle(BanMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new BanMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new BanMemberResult
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
                    return new BanMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to ban/unban members" }
                    };
                }

                // Get member
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.MemberUserId,
                    cancellationToken);

                if (member == null)
                {
                    return new BanMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member not found" }
                    };
                }

                // Cannot ban creator
                if (member.UserId == community.CreatorId)
                {
                    return new BanMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot ban community creator" }
                    };
                }

                // Cannot ban admin/moderator if you're just a moderator
                var currentMember = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.CurrentUserId,
                    cancellationToken);

                if (currentMember != null &&
                    currentMember.Role == CommunityRole.Moderator &&
                    member.Role >= CommunityRole.Moderator)
                {
                    return new BanMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Moderators cannot ban other moderators or admins" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                member.IsBanned = request.IsBanned;
                await _unitOfWork.Communities.UpdateMemberAsync(member, cancellationToken);

                // Update member count if banning approved member
                if (request.IsBanned && member.IsApproved)
                {
                    community.MemberCount = Math.Max(0, community.MemberCount - 1);
                    await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                }
                else if (!request.IsBanned && member.IsApproved)
                {
                    community.MemberCount++;
                    await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var action = request.IsBanned ? "banned" : "unbanned";
                _logger.LogInformation("Member {Action}: {MemberUserId} in community {CommunityId} by {CurrentUserId}",
                    action, request.MemberUserId, request.CommunityId, request.CurrentUserId);

                return new BanMemberResult
                {
                    Success = true,
                    IsBanned = request.IsBanned,
                    Message = $"Member {action} successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error banning/unbanning member {MemberUserId} in community {CommunityId}",
                    request.MemberUserId, request.CommunityId);

                return new BanMemberResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while banning/unbanning member" }
                };
            }
        }
    }

    #endregion

    #region Remove Member Handler

    public class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand, RemoveMemberResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveMemberHandler> _logger;

        public RemoveMemberHandler(
            IUnitOfWork unitOfWork,
            ILogger<RemoveMemberHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RemoveMemberResult> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentUserId))
                {
                    return new RemoveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "User must be logged in" }
                    };
                }

                // Get community
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.CommunityId, cancellationToken);
                if (community == null)
                {
                    return new RemoveMemberResult
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
                    return new RemoveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "You don't have permission to remove members" }
                    };
                }

                // Get member
                var member = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.MemberUserId,
                    cancellationToken);

                if (member == null)
                {
                    return new RemoveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Member not found" }
                    };
                }

                // Cannot remove creator
                if (member.UserId == community.CreatorId)
                {
                    return new RemoveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot remove community creator" }
                    };
                }

                // Moderators cannot remove other moderators or admins
                var currentMember = await _unitOfWork.Communities.GetMemberAsync(
                    request.CommunityId,
                    request.CurrentUserId,
                    cancellationToken);

                if (currentMember != null &&
                    currentMember.Role == CommunityRole.Moderator &&
                    member.Role >= CommunityRole.Moderator)
                {
                    return new RemoveMemberResult
                    {
                        Success = false,
                        Errors = new List<string> { "Moderators cannot remove other moderators or admins" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Remove member
                await _unitOfWork.Communities.RemoveMemberAsync(request.CommunityId, request.MemberUserId, cancellationToken);

                // Update member count if was approved
                if (member.IsApproved)
                {
                    community.MemberCount = Math.Max(0, community.MemberCount - 1);
                    await _unitOfWork.Communities.UpdateAsync(community, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Member removed: {MemberUserId} from community {CommunityId} by {CurrentUserId}",
                    request.MemberUserId, request.CommunityId, request.CurrentUserId);

                return new RemoveMemberResult
                {
                    Success = true,
                    Message = "Member removed successfully"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error removing member {MemberUserId} from community {CommunityId}",
                    request.MemberUserId, request.CommunityId);

                return new RemoveMemberResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while removing member" }
                };
            }
        }
    }

    #endregion
}