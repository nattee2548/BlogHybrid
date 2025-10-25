using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BlogHybrid.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Post interactions
        public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

        // Comment interactions - Legacy
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

        // Comment interactions - NEW
        public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
        public virtual ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();

        // Community
        public virtual ICollection<Community> CreatedCommunities { get; set; } = new List<Community>();
        public virtual ICollection<CommunityMember> CommunityMemberships { get; set; } = new List<CommunityMember>();
        public virtual ICollection<CommunityInvite> SentInvites { get; set; } = new List<CommunityInvite>();
        public virtual ICollection<CommunityInvite> ReceivedInvites { get; set; } = new List<CommunityInvite>();
    }
}