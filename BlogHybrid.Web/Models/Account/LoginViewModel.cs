using System.ComponentModel.DataAnnotations;

namespace BlogHybrid.Web.Models.Account
{
    /// <summary>
    /// ViewModel สำหรับหน้า Login User ทั่วไป
    /// </summary>
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

        [Display(Name = "จดจำฉันไว้")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}