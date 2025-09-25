using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BlogHybrid.Web.Models.ViewModels.Admin
{
    #region User Index

    public class UserIndexViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalUsers { get; set; }
        public int TotalPages { get; set; }
        public List<string> AvailableRoles { get; set; } = new();

        // Helper properties
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartIndex => (CurrentPage - 1) * PageSize + 1;
        public int EndIndex => Math.Min(CurrentPage * PageSize, TotalUsers);

        // Statistics
        public int ActiveUsersCount => Users.Count(u => u.IsActive);
        public int InactiveUsersCount => Users.Count(u => !u.IsActive);
        public int AdminUsersCount => Users.Count(u => u.Roles.Contains("Admin"));
        public int ModeratorUsersCount => Users.Count(u => u.Roles.Contains("Moderator"));
        public int RegularUsersCount => Users.Count(u => !u.Roles.Any() || (!u.Roles.Contains("Admin") && !u.Roles.Contains("Moderator")));
        public int EmailConfirmedCount => Users.Count(u => u.EmailConfirmed);
        public int EmailUnconfirmedCount => Users.Count(u => !u.EmailConfirmed);

        // Search and filter state
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) ||
                                 !string.IsNullOrEmpty(RoleFilter) ||
                                 IsActiveFilter.HasValue;

        public string FilterSummary
        {
            get
            {
                var filters = new List<string>();
                if (!string.IsNullOrEmpty(SearchTerm))
                    filters.Add($"ค้นหา: {SearchTerm}");
                if (!string.IsNullOrEmpty(RoleFilter))
                    filters.Add($"บทบาท: {RoleFilter}");
                if (IsActiveFilter.HasValue)
                    filters.Add($"สถานะ: {(IsActiveFilter.Value ? "ใช้งาน" : "ปิดใช้งาน")}");

                return string.Join(" | ", filters);
            }
        }
    }

    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int AccessFailedCount { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        // Computed properties
        public string FullName => !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
            ? $"{FirstName} {LastName}".Trim()
            : UserName;

        public string DisplayName => !string.IsNullOrEmpty(FullName) && FullName != UserName
            ? $"{FullName} ({UserName})"
            : UserName;

        public string PrimaryRole => Roles.Contains("Admin") ? "Admin"
                                   : Roles.Contains("Moderator") ? "Moderator"
                                   : Roles.FirstOrDefault() ?? "User";

        public string StatusBadgeClass => IsActive ? "success" : "secondary";
        public string StatusText => IsActive ? "ใช้งาน" : "ปิดใช้งาน";

        public string EmailStatusBadgeClass => EmailConfirmed ? "success" : "warning";
        public string EmailStatusText => EmailConfirmed ? "ยืนยันแล้ว" : "ยังไม่ยืนยัน";

        public string RoleBadgeClass => PrimaryRole switch
        {
            "Admin" => "danger",
            "Moderator" => "warning",
            _ => "secondary"
        };

        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;

        public string LoginStatusText => LastLoginAt.HasValue
            ? LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm")
            : "ยังไม่เคยเข้าใช้งาน";

        public string CreatedDateText => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        public string CreatedDateShort => CreatedAt.ToString("dd/MM/yyyy");

        public bool HasMultipleRoles => Roles.Count > 1;

        public bool IsSystemUser => Roles.Contains("Admin") || Roles.Contains("Moderator");

        public bool CanBeDeleted => !IsSystemUser; // Can add more business logic here

        public string SecurityStatusText
        {
            get
            {
                var statuses = new List<string>();
                if (TwoFactorEnabled) statuses.Add("2FA");
                if (IsLockedOut) statuses.Add("ล็อก");
                if (AccessFailedCount > 0) statuses.Add($"ความล้มเหลว: {AccessFailedCount}");

                return statuses.Any() ? string.Join(", ", statuses) : "ปกติ";
            }
        }
    }

    #endregion

    #region User Details

    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
        public List<string> ExternalLogins { get; set; } = new();

        // Computed properties
        public string FullName => !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
            ? $"{FirstName} {LastName}".Trim()
            : UserName;

        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;

        public string StatusText => IsActive ? "ใช้งาน" : "ปิดใช้งาน";
        public string StatusBadgeClass => IsActive ? "success" : "secondary";

        public string EmailStatusText => EmailConfirmed ? "ยืนยันแล้ว" : "ยังไม่ยืนยัน";
        public string EmailStatusBadgeClass => EmailConfirmed ? "success" : "warning";

        public string PhoneStatusText => PhoneNumberConfirmed ? "ยืนยันแล้ว" : "ยังไม่ยืนยัน";
        public string PhoneStatusBadgeClass => PhoneNumberConfirmed ? "success" : "warning";

        public string TwoFactorStatusText => TwoFactorEnabled ? "เปิดใช้งาน" : "ปิดใช้งาน";
        public string TwoFactorStatusBadgeClass => TwoFactorEnabled ? "success" : "secondary";

        public string PrimaryRole => Roles.Contains("Admin") ? "Admin"
                                   : Roles.Contains("Moderator") ? "Moderator"
                                   : "User";

        public bool IsSystemUser => Roles.Contains("Admin") || Roles.Contains("Moderator");

        public bool CanBeModified => true; // Add business logic as needed

        public bool CanBeDeleted => !IsSystemUser;

        public string AccountAgeText
        {
            get
            {
                var age = DateTime.UtcNow - CreatedAt;
                if (age.Days > 365)
                    return $"{age.Days / 365} ปี";
                if (age.Days > 30)
                    return $"{age.Days / 30} เดือน";
                if (age.Days > 0)
                    return $"{age.Days} วัน";
                return "วันนี้";
            }
        }

        public string LastLoginText => LastLoginAt.HasValue
            ? LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm")
            : "ยังไม่เคยเข้าใช้งาน";
    }

    #endregion

    #region Create User

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(100, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย {2} ตัวอักษร", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริง")]
        [StringLength(50, ErrorMessage = "ชื่อจริงต้องมีความยาวไม่เกิน {1} ตัวอักษร")]
        public string? FirstName { get; set; }

        [Display(Name = "นามสกุล")]
        [StringLength(50, ErrorMessage = "นามสกุลต้องมีความยาวไม่เกิน {1} ตัวอักษร")]
        public string? LastName { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ยืนยันอีเมลแล้ว")]
        public bool EmailConfirmed { get; set; } = false;

        [Display(Name = "บทบาท")]
        public List<string>? SelectedRoles { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();

        // Computed properties
        public string FullName => !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
            ? $"{FirstName} {LastName}".Trim()
            : Email;

        public bool HasSelectedRoles => SelectedRoles?.Any() == true;

        public string SelectedRolesText => SelectedRoles?.Any() == true
            ? string.Join(", ", SelectedRoles)
            : "User";
    }

    #endregion

    #region Edit User

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [StringLength(50, ErrorMessage = "ชื่อผู้ใช้ต้องมีความยาวไม่เกิน {1} ตัวอักษร")]
        [Display(Name = "ชื่อผู้ใช้")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "ชื่อจริง")]
        [StringLength(50, ErrorMessage = "ชื่อจริงต้องมีความยาวไม่เกิน {1} ตัวอักษร")]
        public string? FirstName { get; set; }

        [Display(Name = "นามสกุล")]
        [StringLength(50, ErrorMessage = "นามสกุลต้องมีความยาวไม่เกิน {1} ตัวอักษร")]
        public string? LastName { get; set; }

        [Display(Name = "เบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ยืนยันอีเมลแล้ว")]
        public bool EmailConfirmed { get; set; } = false;

        [Display(Name = "บทบาท")]
        public List<string>? SelectedRoles { get; set; } = new();

        public List<SelectListItem> AvailableRoles { get; set; } = new();

        // Computed properties
        public string FullName => !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
            ? $"{FirstName} {LastName}".Trim()
            : UserName;

        public bool HasSelectedRoles => SelectedRoles?.Any() == true;

        public string SelectedRolesText => SelectedRoles?.Any() == true
            ? string.Join(", ", SelectedRoles)
            : "User";

        public string PrimaryRole => SelectedRoles?.Contains("Admin") == true ? "Admin"
                                   : SelectedRoles?.Contains("Moderator") == true ? "Moderator"
                                   : "User";

        public bool IsSystemUser => SelectedRoles?.Contains("Admin") == true ||
                                   SelectedRoles?.Contains("Moderator") == true;
    }

    #endregion

    #region Reset Password

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านใหม่")]
        [StringLength(100, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย {2} ตัวอักษร", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่านใหม่")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่านใหม่")]
        [Compare("NewPassword", ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        // Computed properties
        public string DisplayName => !string.IsNullOrEmpty(UserName) ? UserName : UserEmail;
    }

    #endregion

    #region User Statistics

    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int EmailConfirmedUsers { get; set; }
        public int EmailUnconfirmedUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ModeratorUsers { get; set; }
        public int RegularUsers { get; set; }
        public int LockedUsers { get; set; }
        public int TwoFactorUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public DateTime? LastUserRegistration { get; set; }

        // Computed properties
        public double ActiveUserPercentage => TotalUsers > 0 ? (ActiveUsers * 100.0) / TotalUsers : 0;
        public double EmailConfirmedPercentage => TotalUsers > 0 ? (EmailConfirmedUsers * 100.0) / TotalUsers : 0;
        public double TwoFactorPercentage => TotalUsers > 0 ? (TwoFactorUsers * 100.0) / TotalUsers : 0;

        public string ActiveUserPercentageText => $"{ActiveUserPercentage:F1}%";
        public string EmailConfirmedPercentageText => $"{EmailConfirmedPercentage:F1}%";
        public string TwoFactorPercentageText => $"{TwoFactorPercentage:F1}%";
    }

    #endregion
}