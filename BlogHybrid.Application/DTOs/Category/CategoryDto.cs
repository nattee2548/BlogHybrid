namespace BlogHybrid.Application.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Color { get; set; } = "#0066cc";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PostCount { get; set; }
        public int CommunityCount { get; set; }

        // ========== Hierarchical Properties ==========
        /// <summary>
        /// ID ของหมวดหมู่หลัก (null = หมวดหมู่หลัก)
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// ชื่อหมวดหมู่หลัก
        /// </summary>
        public string? ParentCategoryName { get; set; }

        /// <summary>
        /// เป็นหมวดหมู่หลักหรือไม่
        /// </summary>
        public bool IsParentCategory { get; set; }

        /// <summary>
        /// รายการหมวดหมู่ย่อย (สำหรับ dropdown/tree view)
        /// </summary>
        public List<CategoryDto>? SubCategories { get; set; }

        /// <summary>
        /// จำนวนหมวดหมู่ย่อย
        /// </summary>
        public int SubCategoryCount { get; set; }
    }
}