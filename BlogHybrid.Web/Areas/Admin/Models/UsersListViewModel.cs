using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Areas.Admin.Models
{
    // ViewModel สำหรับแสดงรายการ Users ทั่วไป
    public class UsersListViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // "all", "active", "inactive", "pending"

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // ViewModel สำหรับแต่ละ User ในรายการ
    public class UserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // User Statistics
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunityCount { get; set; }
    }

    // ViewModel สำหรับรายละเอียด User
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Statistics
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunityCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }

        public List<string> Roles { get; set; } = new();
    }

    // ViewModel สำหรับแก้ไข User
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "อีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "ชื่อผู้ใช้")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "ชื่อที่แสดง")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริง")]
        public string? FirstName { get; set; }

        [Display(Name = "นามสกุล")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "ชีวประวัติ")]
        [StringLength(500, ErrorMessage = "ชีวประวัติต้องไม่เกิน 500 ตัวอักษร")]
        public string? Bio { get; set; }

        [Display(Name = "สถานะใช้งาน")]
        public bool IsActive { get; set; }

        [Display(Name = "ยืนยันอีเมล")]
        public bool EmailConfirmed { get; set; }
    }

    // ViewModel สำหรับสร้าง User ใหม่
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [Display(Name = "ชื่อผู้ใช้")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "ชื่อผู้ใช้ต้องมีความยาว 3-50 ตัวอักษร")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริง")]
        public string? FirstName { get; set; }

        [Display(Name = "นามสกุล")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "สถานะใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ยืนยันอีเมลอัตโนมัติ")]
        public bool EmailConfirmed { get; set; } = false;
    }

    // ViewModel สำหรับเปลี่ยนรหัสผ่าน User
    public class ChangeUserPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านใหม่")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่านใหม่")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่านใหม่")]
        [Compare("NewPassword", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}