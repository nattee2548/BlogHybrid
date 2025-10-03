using System;

namespace BlogHybrid.Domain.Entities
{
    public class CommunityInvite
    {
        public int Id { get; set; }
        public int CommunityId { get; set; }
        public string InviterId { get; set; } = string.Empty; // คนที่ส่ง invite
        public string InviteeEmail { get; set; } = string.Empty; // email ที่ถูก invite
        public string? InviteeId { get; set; } // user id (ถ้ามี account แล้ว)
        public string Token { get; set; } = string.Empty; // unique token
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Community Community { get; set; } = null!;
        public virtual ApplicationUser Inviter { get; set; } = null!;
        public virtual ApplicationUser? Invitee { get; set; }
    }
}