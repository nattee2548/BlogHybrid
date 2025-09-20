using BlogHybrid.Application.Queries.Auth;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogHybrid.Application.Handlers.Auth
{
    public class CheckUserExistsHandler : IRequestHandler<CheckUserExistsQuery, CheckUserExistsResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckUserExistsHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CheckUserExistsResult> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
        {
            var result = new CheckUserExistsResult();

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailUser = await _userManager.FindByEmailAsync(request.Email);
                result.EmailExists = emailUser != null;
            }

            if (!string.IsNullOrEmpty(request.DisplayName))
            {
                var displayNameUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DisplayName == request.DisplayName, cancellationToken);
                result.DisplayNameExists = displayNameUser != null;
            }

            return result;
        }
    }
}