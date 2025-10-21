using MediatR;

namespace BlogHybrid.Application.Commands.Community
{
    #region Join Community Command

    public class JoinCommunityCommand : IRequest<JoinCommunityResult>
    {
        public int CommunityId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class JoinCommunityResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool RequiresApproval { get; set; }
    }

    #endregion

    #region Leave Community Command

    public class LeaveCommunityCommand : IRequest<LeaveCommunityResult>
    {
        public int CommunityId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class LeaveCommunityResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion
}