using BlogHybrid.Application.DTOs.Member;
using BlogHybrid.Application.Queries.Member;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogHybrid.Application.Handlers.Member
{
    public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, MemberListDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetMembersQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MemberListDto> Handle(GetMembersQuery request, CancellationToken cancellationToken)
        {
            var query = _userManager.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email!.Contains(request.SearchTerm) ||
                    u.DisplayName.Contains(request.SearchTerm) ||
                    (u.FirstName != null && u.FirstName.Contains(request.SearchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(request.SearchTerm)));
            }

            // Filter by active status
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Sorting
            query = request.SortBy.ToLower() switch
            {
                "email" => request.SortDirection == "asc"
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),
                "displayname" => request.SortDirection == "asc"
                    ? query.OrderBy(u => u.DisplayName)
                    : query.OrderByDescending(u => u.DisplayName),
                _ => request.SortDirection == "asc"
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var members = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var memberDtos = new List<MemberDto>();
            foreach (var member in members)
            {
                var roles = await _userManager.GetRolesAsync(member);

                // Filter by role if specified
                if (!string.IsNullOrWhiteSpace(request.RoleFilter) && !roles.Contains(request.RoleFilter))
                    continue;

                memberDtos.Add(new MemberDto
                {
                    Id = member.Id,
                    Email = member.Email!,
                    UserName = member.UserName,
                    DisplayName = member.DisplayName,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    PhoneNumber = member.PhoneNumber,
                    ProfileImageUrl = member.ProfileImageUrl,
                    Bio = member.Bio,
                    IsActive = member.IsActive,
                    EmailConfirmed = member.EmailConfirmed,
                    CreatedAt = member.CreatedAt,
                    LastLoginAt = member.LastLoginAt,
                    Roles = roles.ToList()
                });
            }

            return new MemberListDto
            {
                Members = memberDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}