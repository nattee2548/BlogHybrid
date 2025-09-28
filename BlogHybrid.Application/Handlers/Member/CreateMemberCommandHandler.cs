using BlogHybrid.Application.Commands.Member;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Member
{
    public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, CreateMemberResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CreateMemberCommandHandler> _logger;

        public CreateMemberCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CreateMemberCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<CreateMemberResult> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if email exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new CreateMemberResult
                    {
                        Success = false,
                        Message = "Email already exists",
                        Errors = new List<string> { "Email is already registered" }
                    };
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Bio = request.Bio,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new CreateMemberResult
                    {
                        Success = false,
                        Message = "Failed to create member",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Assign roles
                if (request.Roles != null && request.Roles.Any())
                {
                    foreach (var roleName in request.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            await _userManager.AddToRoleAsync(user, roleName);
                        }
                    }
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                _logger.LogInformation("Member created successfully: {Email}", request.Email);

                return new CreateMemberResult
                {
                    Success = true,
                    MemberId = user.Id,
                    Message = "Member created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member");
                return new CreateMemberResult
                {
                    Success = false,
                    Message = "An error occurred while creating member",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}