using BlogHybrid.Domain.Enums;
using System;

namespace BlogHybrid.Domain.Entities
{
    public class CommunityMember
    {
        public int CommunityId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public CommunityRole Role { get; set; } = CommunityRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = true; // default approved (ถ้า RequireApproval = false)
        public bool IsBanned { get; set; } = false;

        // Navigation properties
        public virtual Community Community { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}