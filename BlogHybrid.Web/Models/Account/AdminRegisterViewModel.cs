using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.Account
{
    public class AdminRegisterViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
        [StringLength(50, ErrorMessage = "ชื่อที่แสดงต้องมีความยาว 3-50 ตัวอักษร", MinimumLength = 3)]
        [Display(Name = "ชื่อที่แสดง")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริง")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Display(Name = "นามสกุล")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(100, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายอมรับข้อกำหนดและเงื่อนไข")]
        [Display(Name = "ยอมรับข้อกำหนดและเงื่อนไข")]
        public bool AcceptTerms { get; set; }
    }
}




