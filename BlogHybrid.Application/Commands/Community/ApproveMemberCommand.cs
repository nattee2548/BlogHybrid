using BlogHybrid.Domain.Enums;
using MediatR;

namespace BlogHybrid.Application.Commands.Community
{
    #region Approve Member Command

    public class ApproveMemberCommand : IRequest<ApproveMemberResult>
    {
        public int CommunityId { get; set; }
        public string MemberUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty; // Admin/Moderator who approves
    }

    public class ApproveMemberResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Reject Member Command

    public class RejectMemberCommand : IRequest<RejectMemberResult>
    {
        public int CommunityId { get; set; }
        public string MemberUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class RejectMemberResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion

    #region Change Member Role Command

    public class ChangeMemberRoleCommand : IRequest<ChangeMemberRoleResult>
    {
        public int CommunityId { get; set; }
        public string MemberUserId { get; set; } = string.Empty;
        public CommunityRole NewRole { get; set; }
        public string CurrentUserId { get; set; } = string.Empty; // Admin who changes role
    }

    public class ChangeMemberRoleResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public CommunityRole? NewRole { get; set; }
    }

    #endregion

    #region Ban Member Command

    public class BanMemberCommand : IRequest<BanMemberResult>
    {
        public int CommunityId { get; set; }
        public string MemberUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public bool IsBanned { get; set; } = true; // true = ban, false = unban
    }

    public class BanMemberResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsBanned { get; set; }
    }

    #endregion

    #region Remove Member Command

    public class RemoveMemberCommand : IRequest<RemoveMemberResult>
    {
        public int CommunityId { get; set; }
        public string MemberUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class RemoveMemberResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion
}