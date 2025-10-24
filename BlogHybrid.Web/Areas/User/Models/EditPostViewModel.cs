// ============================================================
// EditPostViewModel.cs - FULL VERSION (แนวทาง 2)
// Location: BlogHybrid.Web/Areas/User/Models/
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Areas.User.Models
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณาระบุหัวข้อโพสต์")]
        [StringLength(200, ErrorMessage = "หัวข้อต้องไม่เกิน 200 ตัวอักษร")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุเนื้อหาโพสต์")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "คำอธิบายย่อต้องไม่เกิน 500 ตัวอักษร")]
        public string? Excerpt { get; set; }

        public string? FeaturedImageUrl { get; set; }
        public string? CurrentFeaturedImageUrl { get; set; }

        [Display(Name = "หมวดหมู่")]
        public int? CategoryId { get; set; }

        [Display(Name = "ชุมชน")]
        public int? CommunityId { get; set; }

        [Display(Name = "แท็ก (คั่นด้วยจุลภาค)")]
        public string? Tags { get; set; }

        [Display(Name = "เผยแพร่")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "โพสต์แนะนำ")]
        public bool IsFeatured { get; set; } = false;

        // ⭐ Properties เพิ่มเติมสำหรับ UI (ถ้าต้องการ)
        public string? OriginalSlug { get; set; }
        public string? CurrentCategoryName { get; set; }
        public string? CurrentCommunityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ⭐ Collections สำหรับ Dropdowns (ถ้าไม่ใช้ ViewBag)
        public List<CategoryDto> Categories { get; set; } = new();
        public List<CommunityDto> Communities { get; set; } = new();
    }

    // DTOs สำหรับ Dropdowns
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CommunityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}