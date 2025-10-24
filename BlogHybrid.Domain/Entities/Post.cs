using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlogHybrid.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        // Foreign Keys
        public string AuthorId { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int? CommunityId { get; set; } // ⭐ NEW - nullable (post อาจไม่อยู่ใน community)

        // Navigation properties
        public virtual ApplicationUser Author { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual Community? Community { get; set; } // ⭐ NEW
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    }
}