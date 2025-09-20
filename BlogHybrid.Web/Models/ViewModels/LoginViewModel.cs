// BlogHybrid.Web/Models/ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "จดจำการเข้าสู่ระบบ")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}