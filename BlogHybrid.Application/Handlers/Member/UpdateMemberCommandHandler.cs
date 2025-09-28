using BlogHybrid.Application.Commands.Member;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Member
{
    public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, UpdateMemberResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UpdateMemberCommandHandler> _logger;

        public UpdateMemberCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UpdateMemberCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<UpdateMemberResult> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.Id);

                if (user == null)
                {
                    return new UpdateMemberResult
                    {
                        Success = false,
                        Message = "Member not found"
                    };
                }

                // Update user info
                user.DisplayName = request.DisplayName;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.Bio = request.Bio;
                user.IsActive = request.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new UpdateMemberResult
                    {
                        Success = false,
                        Message = "Failed to update member",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Update roles
                if (request.Roles != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    foreach (var roleName in request.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            await _userManager.AddToRoleAsync(user, roleName);
                        }
                    }
                }

                _logger.LogInformation("Member updated successfully: {Id}", request.Id);

                return new UpdateMemberResult
                {
                    Success = true,
                    Message = "Member updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member");
                return new UpdateMemberResult
                {
                    Success = false,
                    Message = "An error occurred while updating member",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}