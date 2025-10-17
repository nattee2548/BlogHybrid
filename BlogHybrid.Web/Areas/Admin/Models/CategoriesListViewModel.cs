// BlogHybrid.Web/Areas/Admin/Models/CategoryViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Areas.Admin.Models
{
    // ViewModel สำหรับแสดงรายการ Categories
    public class CategoriesListViewModel
    {
        public List<CategoryItemViewModel> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string? SearchTerm { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // ViewModel สำหรับแต่ละ Category ในรายการ
    public class CategoryItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#0066cc";
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int PostCount { get; set; }
        public int CommunityCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ViewModel สำหรับรายละเอียด Category
    public class CategoryDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#0066cc";
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int PostCount { get; set; }
        public int CommunityCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ViewModel สำหรับสร้าง Category
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [StringLength(100, ErrorMessage = "ชื่อหมวดหมู่ต้องไม่เกิน 100 ตัวอักษร")]
        [Display(Name = "ชื่อหมวดหมู่")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกคำอธิบาย")]
        [StringLength(500, ErrorMessage = "คำอธิบายต้องไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกสี")]
        [Display(Name = "สีประจำหมวดหมู่")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "รูปแบบสีไม่ถูกต้อง (ต้องเป็น HEX เช่น #0066cc)")]
        public string Color { get; set; } = "#0066cc";

        [Display(Name = "URL รูปภาพ")]
        [Url(ErrorMessage = "รูปแบบ URL ไม่ถูกต้อง")]
        public string? ImageUrl { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ลำดับการแสดงผล")]
        [Range(0, 9999, ErrorMessage = "ลำดับต้องอยู่ระหว่าง 0-9999")]
        public int SortOrder { get; set; } = 0;
    }

    // ViewModel สำหรับแก้ไข Category
    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [StringLength(100, ErrorMessage = "ชื่อหมวดหมู่ต้องไม่เกิน 100 ตัวอักษร")]
        [Display(Name = "ชื่อหมวดหมู่")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกคำอธิบาย")]
        [StringLength(500, ErrorMessage = "คำอธิบายต้องไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกสี")]
        [Display(Name = "สีประจำหมวดหมู่")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "รูปแบบสีไม่ถูกต้อง (ต้องเป็น HEX เช่น #0066cc)")]
        public string Color { get; set; } = "#0066cc";

        [Display(Name = "URL รูปภาพ")]
        [Url(ErrorMessage = "รูปแบบ URL ไม่ถูกต้อง")]
        public string? ImageUrl { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; }

        [Display(Name = "ลำดับการแสดงผล")]
        [Range(0, 9999, ErrorMessage = "ลำดับต้องอยู่ระหว่าง 0-9999")]
        public int SortOrder { get; set; }
    }
}