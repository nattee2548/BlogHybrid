using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.ViewModels.Admin
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [StringLength(100, ErrorMessage = "ชื่อหมวดหมู่ต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
        [Display(Name = "ชื่อหมวดหมู่")]
        public string Name { get; set; } = string.Empty;
        [StringLength(100, ErrorMessage = "Slug ต้องไม่เกิน 100 ตัวอักษร")]
        public string? Slug { get; set; }  
        [StringLength(500, ErrorMessage = "คำอธิบายต้องมีความยาวไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "URL รูปภาพต้องมีความยาวไม่เกิน 255 ตัวอักษร")]
        //[Url(ErrorMessage = "รูปแบบ URL ไม่ถูกต้อง")]
        [Display(Name = "URL รูปภาพ")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกสี")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "รูปแบบสีไม่ถูกต้อง")]
        [Display(Name = "สีประจำหมวดหมู่")]
        public string Color { get; set; } = "#0066cc";

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Range(0, 999, ErrorMessage = "ลำดับการแสดงต้องอยู่ระหว่าง 0-999")]
        [Display(Name = "ลำดับการแสดง")]
        public int SortOrder { get; set; }

        // Helper properties
        public string PreviewSlug => GenerateSlug(Name);

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Simple slug generation for preview
            return name.ToLowerInvariant()
                      .Replace(" ", "-")
                      .Replace("ำ", "am")
                      .Replace("ใ", "ai")
                      .Replace("ไ", "ai");
        }
    }
}
