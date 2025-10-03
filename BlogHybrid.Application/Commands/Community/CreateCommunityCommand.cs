using MediatR;

namespace BlogHybrid.Application.Commands.Community
{
    public class CreateCommunityCommand : IRequest<CreateCommunityResult>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }
        public int CategoryId { get; set; }
        public bool IsPrivate { get; set; } = false;
        public bool RequireApproval { get; set; } = false;

        // จะถูก set จาก Controller (Current User)
        public string? CreatorId { get; set; }
    }

    public class CreateCommunityResult
    {
        public bool Success { get; set; }
        public int? CommunityId { get; set; }
        public string? Slug { get; set; }
        public string? FullSlug { get; set; } // category-slug/community-slug
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}