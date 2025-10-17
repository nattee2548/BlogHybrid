using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Areas.Admin.Models
{
    // ViewModel สำหรับแสดงรายการ Admin Users
    public class AdminUsersListViewModel
    {
        public List<AdminUserItemViewModel> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // ViewModel สำหรับแต่ละ Admin User ในรายการ
    public class AdminUserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    // ViewModel สำหรับรายละเอียด Admin User
    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName {  get; set; } = string.Empty;        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    // ViewModel สำหรับแก้ไข Admin User
    public class EditAdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "ชื่อผู้ใช้")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อจริง")]
        [Display(Name = "ชื่อจริง")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกนามสกุล")]
        [Display(Name = "นามสกุล")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "สถานะการใช้งาน")]
        public bool IsActive { get; set; }

        [Display(Name = "ยืนยันอีเมลแล้ว")]
        public bool EmailConfirmed { get; set; }
    }
}