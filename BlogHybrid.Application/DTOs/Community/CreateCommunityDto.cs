namespace BlogHybrid.Application.DTOs.Community
{
    public class CreateCommunityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }
        public int CategoryId { get; set; }
        public bool IsPrivate { get; set; } = false;
        public bool RequireApproval { get; set; } = false;
    }

    public class UpdateCommunityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }
        public int CategoryId { get; set; } // ⭐ ตาม requirement ให้เปลี่ยนได้
        public bool IsPrivate { get; set; }
        public bool RequireApproval { get; set; }
        public bool IsActive { get; set; }
    }

    public class CommunityStatsDto
    {
        public int TotalCommunities { get; set; }
        public int ActiveCommunities { get; set; }
        public int PrivateCommunities { get; set; }
        public int PublicCommunities { get; set; }
        public int TotalMembers { get; set; }
        public int TotalPosts { get; set; }
        public List<CategoryCommunityStat> CommunitiesByCategory { get; set; } = new();
    }

    public class CategoryCommunityStat
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CommunityCount { get; set; }
    }

    public class CommunityMemberDto
    {
        public int CommunityId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string? UserProfileImageUrl { get; set; }
        public string Role { get; set; } = string.Empty; // Member, Moderator, Admin
        public DateTime JoinedAt { get; set; }
        public bool IsApproved { get; set; }
        public bool IsBanned { get; set; }
    }
}