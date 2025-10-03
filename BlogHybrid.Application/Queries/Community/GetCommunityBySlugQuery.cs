using BlogHybrid.Application.DTOs.Community;
using MediatR;

namespace BlogHybrid.Application.Queries.Community
{
    public class GetCommunityBySlugQuery : IRequest<CommunityDto?>
    {
        public string Slug { get; set; } = string.Empty;

        // Current user (for checking membership & role)
        public string? CurrentUserId { get; set; }

        // Include deleted communities (for admin)
        public bool IncludeDeleted { get; set; } = false;
    }

    public class CheckUserCommunityLimitQuery : IRequest<CheckUserCommunityLimitResult>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class CheckUserCommunityLimitResult
    {
        public bool CanCreateMore { get; set; }
        public int CurrentCount { get; set; }
        public int MaxAllowed { get; set; }
        public int Remaining => MaxAllowed - CurrentCount;
    }

    public class CheckCommunitySlugExistsQuery : IRequest<bool>
    {
        public string Slug { get; set; } = string.Empty;
        public int? ExcludeId { get; set; }
    }

    public class GetCommunityStatsQuery : IRequest<CommunityStatsDto>
    {
        public int? CategoryId { get; set; } // null = all categories
    }
}