using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.ViewModels.Admin
{
    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [StringLength(100, ErrorMessage = "ชื่อหมวดหมู่ต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
        [Display(Name = "ชื่อหมวดหมู่")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "คำอธิบายต้องมีความยาวไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "URL รูปภาพต้องมีความยาวไม่เกิน 255 ตัวอักษร")]
        [Url(ErrorMessage = "รูปแบบ URL ไม่ถูกต้อง")]
        [Display(Name = "URL รูปภาพ")]
        public string? ImageUrl { get; set; }
        public string? FullImageUrl { get; set; }
        [Required(ErrorMessage = "กรุณาเลือกสี")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "รูปแบบสีไม่ถูกต้อง")]
        [Display(Name = "สีประจำหมวดหมู่")]
        public string Color { get; set; } = "#0066cc";

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Range(0, 999, ErrorMessage = "ลำดับการแสดงต้องอยู่ระหว่าง 0-999")]
        [Display(Name = "ลำดับการแสดง")]
        public int SortOrder { get; set; }

        // Read-only properties
        public string CurrentSlug { get; set; } = string.Empty;
        public int PostCount { get; set; }

        // Helper properties
        public bool HasPosts => PostCount > 0;
        public string StatusBadge => IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
        public string StatusClass => IsActive ? "bg-success" : "bg-secondary";
    }
}
