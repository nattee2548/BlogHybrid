using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Areas.User.Models
{
    /// <summary>
    /// ViewModel สำหรับแสดงโปรไฟล์ผู้ใช้
    /// </summary>
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Statistics
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunityCount { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับแก้ไขโปรไฟล์
    /// </summary>
    public class EditProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
        [StringLength(50, ErrorMessage = "ชื่อที่แสดงต้องมีความยาว 3-50 ตัวอักษร", MinimumLength = 3)]
        [Display(Name = "ชื่อที่แสดง")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "ชื่อจริงต้องไม่เกิน 50 ตัวอักษร")]
        [Display(Name = "ชื่อจริง")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "นามสกุลต้องไม่เกิน 50 ตัวอักษร")]
        [Display(Name = "นามสกุล")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string? PhoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "Bio ต้องไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "แนะนำตัว")]
        public string? Bio { get; set; }

        [Display(Name = "รูปโปรไฟล์")]
        public IFormFile? ProfileImage { get; set; }

        public string? CurrentProfileImageUrl { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับเปลี่ยนรหัสผ่าน
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านปัจจุบัน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่านปัจจุบัน")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านใหม่")]
        [StringLength(100, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่านใหม่")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่านใหม่")]
        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่านใหม่")]
        [Compare("NewPassword", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}