// BlogHybrid.Web/Models/ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อที่แสดง")]
        [StringLength(100, ErrorMessage = "ชื่อที่แสดงต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
        [Display(Name = "ชื่อที่แสดง")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [StringLength(255, ErrorMessage = "อีเมลต้องมีความยาวไม่เกิน 255 ตัวอักษร")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(100, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย {2} ตัวอักษร", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัسผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "ยอมรับข้อกำหนดและเงื่อนไข")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "กรุณายอมรับข้อกำหนดและเงื่อนไข")]
        public bool AcceptTerms { get; set; }
    }
}