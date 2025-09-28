using BlogHybrid.Application.DTOs.Member;
using BlogHybrid.Application.Queries.Member;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BlogHybrid.Application.Handlers.Member
{
    public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto?>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetMemberByIdQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MemberDto?> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
        {
            var member = await _userManager.FindByIdAsync(request.Id);

            if (member == null)
                return null;

            var roles = await _userManager.GetRolesAsync(member);

            return new MemberDto
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
            };
        }
    }
}