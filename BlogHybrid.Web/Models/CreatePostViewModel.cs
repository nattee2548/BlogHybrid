using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.Post
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกหัวข้อโพสต์")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "หัวข้อโพสต์ต้องมีความยาว 3-200 ตัวอักษร")]
        [Display(Name = "หัวข้อโพสต์")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเนื้อหาโพสต์")]
        [MinLength(10, ErrorMessage = "เนื้อหาโพสต์ต้องมีความยาวอย่างน้อย 10 ตัวอักษร")]
        [Display(Name = "เนื้อหาโพสต์")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "คำอธิบายย่อต้องไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "คำอธิบายย่อ")]
        public string? Excerpt { get; set; }

        [Display(Name = "รูปภาพประกอบ")]
        public string? FeaturedImageUrl { get; set; }

        // ⭐ Optional - ต้องเลือกอย่างน้อย 1 อย่าง
        [Display(Name = "หมวดหมู่")]
        public int? CategoryId { get; set; }

        // ⭐ Optional - ต้องเลือกอย่างน้อย 1 อย่าง
        [Display(Name = "ชุมชน")]
        public int? CommunityId { get; set; }

        [Display(Name = "แท็ก")]
        public string? Tags { get; set; }

        [Display(Name = "เผยแพร่ทันที")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "โพสต์แนะนำ")]
        public bool IsFeatured { get; set; } = false;

        // === UI Helper Properties ===

        /// <summary>
        /// ถ้าสร้างจากหน้าชุมชน จะ lock community และไม่ให้แก้ไข
        /// </summary>
        public bool IsFromCommunity { get; set; } = false;

        /// <summary>
        /// ชื่อชุมชน (สำหรับแสดงเมื่อ lock)
        /// </summary>
        public string? CommunityName { get; set; }

        /// <summary>
        /// รายการหมวดหมู่สำหรับ dropdown
        /// </summary>
        public List<CategorySelectItem> Categories { get; set; } = new();

        /// <summary>
        /// รายการชุมชนที่ user เป็นสมาชิก สำหรับ dropdown
        /// </summary>
        public List<CommunitySelectItem> Communities { get; set; } = new();
    }

    /// <summary>
    /// Item สำหรับ Category Dropdown
    /// </summary>
    public class CategorySelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }

        /// <summary>
        /// Display name for dropdown (รวม parent name ถ้ามี)
        /// </summary>
        public string DisplayName => ParentCategoryName != null
            ? $"{ParentCategoryName} > {Name}"
            : Name;
    }

    /// <summary>
    /// Item สำหรับ Community Dropdown
    /// </summary>
    public class CommunitySelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int MemberCount { get; set; }
        public bool IsPrivate { get; set; }
    }
}