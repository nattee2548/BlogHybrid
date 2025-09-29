using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Application.DTOs.Auth;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _tokenService = tokenService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Login user and get JWT tokens
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "เข้าสู่ระบบไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                // Get user for token generation
                var user = await _userManager.FindByEmailAsync(command.Email);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ไม่พบข้อมูลผู้ใช้หลังจากเข้าสู่ระบบสำเร็จ"
                    });
                }

                // Generate JWT tokens
                var tokenDto = await _tokenService.GenerateTokenAsync(user);

                _logger.LogInformation("User logged in successfully: {Email}", command.Email);

                return Ok(new
                {
                    success = true,
                    message = "เข้าสู่ระบบสำเร็จ",
                    token = tokenDto,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        displayName = user.DisplayName,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        isActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง"
                });
            }
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Refresh token is required"
                    });
                }

                var ipAddress = GetIpAddress();
                var result = await _tokenService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "รีเฟรชโทเค็นสำเร็จ",
                    token = result.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการรีเฟรชโทเค็น"
                });
            }
        }

        private string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            return ipAddress ?? "Unknown";
        }
    }
}