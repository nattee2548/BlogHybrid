using BlogHybrid.Application.DTOs.Community;
using MediatR;

namespace BlogHybrid.Application.Queries.Community
{
    public class GetCommunitiesQuery : IRequest<CommunityListDto>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsPrivate { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";

        // Current user (for checking membership)
        public string? CurrentUserId { get; set; }
    }

    public class GetCommunitiesByCategoryQuery : IRequest<List<CommunityDto>>
    {
        public int CategoryId { get; set; }
        public bool IncludePrivate { get; set; } = false;
        public bool OnlyActive { get; set; } = true;

        // Current user
        public string? CurrentUserId { get; set; }
    }

    public class GetUserCommunitiesQuery : IRequest<List<CommunityDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public bool IncludeDeleted { get; set; } = false;
    }

    public class GetPopularCommunitiesQuery : IRequest<List<CommunityDto>>
    {
        public int Top { get; set; } = 10;
        public int? CategoryId { get; set; }
        public bool OnlyPublic { get; set; } = true;
    }
}