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

        // ✨ เปลี่ยนจาก Optional เป็น Required
        [Required(ErrorMessage = "กรุณากรอกชื่อจริง")]
        [StringLength(50, ErrorMessage = "ชื่อจริงต้องมีความยาวไม่เกิน 50 ตัวอักษร")]
        [Display(Name = "ชื่อจริง")]
        public string FirstName { get; set; } = string.Empty;

        // ✨ เปลี่ยนจาก Optional เป็น Required
        [Required(ErrorMessage = "กรุณากรอกนามสกุล")]
        [StringLength(50, ErrorMessage = "นามสกุลต้องมีความยาวไม่เกิน 50 ตัวอักษร")]
        [Display(Name = "นามสกุล")]
        public string LastName { get; set; } = string.Empty;

        // ✨ เปลี่ยนจาก Optional เป็น Required และเพิ่ม Validation
        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "เบอร์โทรศัพท์ต้องเป็นตัวเลข 10 หลัก")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string PhoneNumber { get; set; } = string.Empty;

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
