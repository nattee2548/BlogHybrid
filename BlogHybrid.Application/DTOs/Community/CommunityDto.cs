using BlogHybrid.Domain.Enums;

namespace BlogHybrid.Application.DTOs.Community
{
    public class CommunityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }

        // Status
        public bool IsActive { get; set; }
        public bool IsPrivate { get; set; }
        public bool RequireApproval { get; set; }

        // Statistics
        public int MemberCount { get; set; }
        public int PostCount { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;

        // Creator info
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorDisplayName { get; set; } = string.Empty;

        // Current user's status (จะ set ใน handler)
        public bool? IsCurrentUserMember { get; set; }
        public CommunityRole? CurrentUserRole { get; set; }

        // Full URL path (for frontend)
        public string FullSlug => $"{CategorySlug}/{Slug}";
    }
}