// BlogHybrid.Domain/Entities/Tag.cs
using System;
using System.Collections.Generic;

namespace BlogHybrid.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
        public string? CreatedBy { get; set; }  // User ID ที่สร้าง (nullable สำหรับ system tags)

   
        public virtual ApplicationUser? Creator { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}