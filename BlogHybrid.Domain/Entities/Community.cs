using System;
using System.Collections.Generic;

namespace BlogHybrid.Domain.Entities
{
    public class Community
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Rules { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public bool IsPrivate { get; set; } = false;
        public bool RequireApproval { get; set; } = false;

        // Statistics
        public int MemberCount { get; set; } = 0;
        public int PostCount { get; set; } = 0;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        public int CategoryId { get; set; }
        public string CreatorId { get; set; } = string.Empty;

        // For ordering
        public int SortOrder { get; set; } = 0;

        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ICollection<CommunityMember> CommunityMembers { get; set; } = new List<CommunityMember>();
        public virtual ICollection<CommunityInvite> CommunityInvites { get; set; } = new List<CommunityInvite>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<CommunityCategory> CommunityCategories { get; set; } = new List<CommunityCategory>();
    }
}