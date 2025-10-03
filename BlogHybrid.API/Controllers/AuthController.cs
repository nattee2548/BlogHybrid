// Path: BlogHybrid.API/Controllers/AuthController.cs
using BlogHybrid.Application.Commands.Auth;
using BlogHybrid.Application.DTOs.Auth;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        /// Register new user (Public)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ข้อมูลไม่ถูกต้อง",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "สมัครสมาชิกไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("User registered successfully: {Email}", command.Email);

                return CreatedAtAction(
                    nameof(Register),
                    new { id = result.UserId },
                    new
                    {
                        success = true,
                        userId = result.UserId,
                        message = result.Message,
                        errors = new List<string>()
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง",
                    errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Login user and get JWT tokens
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
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
                var result = await _tokenService.RevokeTokenAsync(
                    request.RefreshToken,
                    ipAddress,
                    "User logout"
                );

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to revoke token"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "ออกจากระบบสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการออกจากระบบ"
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