using BlogHybrid.Domain.Entities;
using MediatR;

namespace BlogHybrid.Application.Queries.User
{
    #region Get User By Id

    public class GetUserByIdQuery : IRequest<ApplicationUser?>
    {
        public string Id { get; set; } = string.Empty;
    }

    #endregion

    #region Get User By Email

    public class GetUserByEmailQuery : IRequest<ApplicationUser?>
    {
        public string Email { get; set; } = string.Empty;
    }

    #endregion

    #region Get Paged Users

    public class GetPagedUsersQuery : IRequest<GetPagedUsersResult>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }

    public class GetPagedUsersResult
    {
        public List<UserListItem> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; // ✅ เพิ่ม DisplayName
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    #endregion

    #region Get User Details

    public class GetUserDetailsQuery : IRequest<UserDetailsResult?>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class UserDetailsResult
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    #endregion

    #region Get All Roles

    public class GetAllRolesQuery : IRequest<List<string>>
    {
    }

    #endregion

    #region Get User Statistics

    public class GetUserStatisticsQuery : IRequest<UserStatisticsResult>
    {
    }

    public class UserStatisticsResult
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int EmailConfirmedUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ModeratorUsers { get; set; }
        public int RegularUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public DateTime? LastUserRegistration { get; set; }
    }

    #endregion

    #region Check Email Exists

    public class CheckEmailExistsQuery : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string? ExcludeUserId { get; set; }
    }

    #endregion

    #region Check UserName Exists

    public class CheckUserNameExistsQuery : IRequest<bool>
    {
        public string UserName { get; set; } = string.Empty;
        public string? ExcludeUserId { get; set; }
    }

    #endregion
}