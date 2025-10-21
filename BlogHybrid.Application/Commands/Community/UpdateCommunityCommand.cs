using MediatR;

namespace BlogHybrid.Application.Commands.Community
{
    public class UpdateCommunityCommand : IRequest<UpdateCommunityResult>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }
        public string CategoryIds { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public bool RequireApproval { get; set; }
        public bool IsActive { get; set; }
        public bool IsNSFW { get; set; } = false;
        // จะถูก set จาก Controller (Current User)
        public string? CurrentUserId { get; set; }
    }

    public class UpdateCommunityResult
    {
        public bool Success { get; set; }
        public string? Slug { get; set; }
        public string? FullSlug { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}