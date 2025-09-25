using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.User;
using BlogHybrid.Domain.Entities; // แก้ไขจาก Core.Entities เป็น Domain.Entities
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.User
{
    #region Get User By Id Handler

    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ApplicationUser?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApplicationUser?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
        }
    }

    #endregion

    #region Get User By Email Handler

    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, ApplicationUser?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByEmailHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApplicationUser?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        }
    }

    #endregion

    #region Get Paged Users Handler

    public class GetPagedUsersHandler : IRequestHandler<GetPagedUsersQuery, GetPagedUsersResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPagedUsersHandler> _logger;

        public GetPagedUsersHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPagedUsersHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GetPagedUsersResult> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (users, totalCount) = await _unitOfWork.Users.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    request.RoleFilter,
                    request.IsActiveFilter,
                    request.SortBy,
                    request.SortDirection,
                    cancellationToken);

                var userListItems = new List<UserListItem>();

                foreach (var user in users)
                {
                    var roles = await _unitOfWork.Users.GetRolesAsync(user, cancellationToken);

                    userListItems.Add(new UserListItem
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        UserName = user.UserName!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        Roles = roles
                    });
                }

                return new GetPagedUsersResult
                {
                    Users = userListItems,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged users");
                return new GetPagedUsersResult
                {
                    Users = new List<UserListItem>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }

    #endregion

    #region Get User Details Handler

    public class GetUserDetailsHandler : IRequestHandler<GetUserDetailsQuery, UserDetailsResult?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUserDetailsHandler> _logger;

        public GetUserDetailsHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetUserDetailsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserDetailsResult?> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
                if (user == null)
                {
                    return null;
                }

                var roles = await _unitOfWork.Users.GetRolesAsync(user, cancellationToken);
                var claims = await _unitOfWork.Users.GetClaimsAsync(user, cancellationToken);
                var logins = await _unitOfWork.Users.GetLoginsAsync(user, cancellationToken);

                return new UserDetailsResult
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Bio = user.Bio,
                    Roles = roles,
                    Claims = claims.ToDictionary(c => c.Type, c => c.Value),
                    ExternalLogins = logins.Select(l => l.LoginProvider).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for ID: {UserId}", request.Id);
                return null;
            }
        }
    }

    #endregion

    #region Get All Roles Handler

    public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllRolesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<string>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.GetAllRolesAsync(cancellationToken);
        }
    }

    #endregion

    #region Get User Statistics Handler

    public class GetUserStatisticsHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUserStatisticsHandler> _logger;

        public GetUserStatisticsHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetUserStatisticsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserStatisticsResult> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var totalUsers = await _unitOfWork.Users.GetTotalUsersCountAsync(cancellationToken);
                var activeUsers = await _unitOfWork.Users.GetActiveUsersCountAsync(cancellationToken);
                var adminUsers = await _unitOfWork.Users.GetUsersCountByRoleAsync("Admin", cancellationToken);
                var moderatorUsers = await _unitOfWork.Users.GetUsersCountByRoleAsync("Moderator", cancellationToken);

                var thisMonth = DateTime.UtcNow.AddDays(-30);
                var thisWeek = DateTime.UtcNow.AddDays(-7);

                var newUsersThisMonth = await _unitOfWork.Users.GetNewUsersCountAsync(thisMonth, cancellationToken);
                var newUsersThisWeek = await _unitOfWork.Users.GetNewUsersCountAsync(thisWeek, cancellationToken);
                var lastUserRegistration = await _unitOfWork.Users.GetLastUserRegistrationDateAsync(cancellationToken);

                return new UserStatisticsResult
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    EmailConfirmedUsers = 0, // TODO: Implement if needed
                    AdminUsers = adminUsers,
                    ModeratorUsers = moderatorUsers,
                    RegularUsers = totalUsers - adminUsers - moderatorUsers,
                    NewUsersThisMonth = newUsersThisMonth,
                    NewUsersThisWeek = newUsersThisWeek,
                    LastUserRegistration = lastUserRegistration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return new UserStatisticsResult();
            }
        }
    }

    #endregion

    #region Check Email Exists Handler

    public class CheckEmailExistsHandler : IRequestHandler<CheckEmailExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckEmailExistsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckEmailExistsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.EmailExistsAsync(request.Email, request.ExcludeUserId, cancellationToken);
        }
    }

    #endregion

    #region Check UserName Exists Handler

    public class CheckUserNameExistsHandler : IRequestHandler<CheckUserNameExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckUserNameExistsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckUserNameExistsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Users.UserNameExistsAsync(request.UserName, request.ExcludeUserId, cancellationToken);
        }
    }

    #endregion
}