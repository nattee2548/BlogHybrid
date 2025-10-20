using System;
using System.Collections.Generic;

namespace BlogHybrid.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Color { get; set; } = "#0066cc";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ========== Hierarchical Structure ==========
        /// <summary>
        /// ID ของหมวดหมู่หลัก (null = หมวดหมู่หลัก)
        /// </summary>
        public int? ParentCategoryId { get; set; }

        // Navigation properties
        /// <summary>
        /// หมวดหมู่หลัก (Parent)
        /// </summary>
        public virtual Category? ParentCategory { get; set; }

        /// <summary>
        /// หมวดหมู่ย่อย (Children/Subcategories)
        /// </summary>
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // Existing navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        //public virtual ICollection<Community> Communities { get; set; } = new List<Community>();
        public virtual ICollection<CommunityCategory> CommunityCategories { get; set; } = new List<CommunityCategory>();
        // ========== Helper Properties ==========
        /// <summary>
        /// เช็คว่าเป็นหมวดหมู่หลักหรือไม่
        /// </summary>
        public bool IsParentCategory => ParentCategoryId == null;

        /// <summary>
        /// เช็คว่าเป็นหมวดหมู่ย่อยหรือไม่
        /// </summary>
        public bool IsSubCategory => ParentCategoryId != null;

        /// <summary>
        /// จำนวนหมวดหมู่ย่อย
        /// </summary>
        public int SubCategoryCount => SubCategories?.Count ?? 0;
    }
}