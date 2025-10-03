using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Application.Queries.Community;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHybrid.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunitiesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CommunitiesController> _logger;

        public CommunitiesController(IMediator mediator, ILogger<CommunitiesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        #region Query Endpoints

        // GET: api/communities
        [HttpGet]
        public async Task<IActionResult> GetCommunities([FromQuery] GetCommunitiesQuery query)
        {
            query.CurrentUserId = GetCurrentUserId();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/communities/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommunity(int id)
        {
            var query = new GetCommunityByIdQuery
            {
                Id = id,
                CurrentUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "Community not found or you don't have access" });

            return Ok(result);
        }

        // GET: api/communities/slug/{slug}
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetCommunityBySlug(string slug)
        {
            var query = new GetCommunityBySlugQuery
            {
                Slug = slug,
                CurrentUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "Community not found or you don't have access" });

            return Ok(result);
        }

        // GET: api/communities/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetCommunitiesByCategory(int categoryId, [FromQuery] bool includePrivate = false, [FromQuery] bool onlyActive = true)
        {
            var query = new GetCommunitiesByCategoryQuery
            {
                CategoryId = categoryId,
                IncludePrivate = includePrivate,
                OnlyActive = onlyActive,
                CurrentUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/communities/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCommunities(string userId, [FromQuery] bool includeDeleted = false)
        {
            var query = new GetUserCommunitiesQuery
            {
                UserId = userId,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/communities/popular
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularCommunities([FromQuery] int top = 10, [FromQuery] int? categoryId = null)
        {
            var query = new GetPopularCommunitiesQuery
            {
                Top = top,
                CategoryId = categoryId,
                OnlyPublic = true
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/communities/check-limit
        [HttpGet("check-limit")]
        [Authorize]
        public async Task<IActionResult> CheckCommunityLimit()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new CheckUserCommunityLimitQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/communities/check-slug/{slug}
        [HttpGet("check-slug/{slug}")]
        public async Task<IActionResult> CheckSlugExists(string slug, [FromQuery] int? excludeId = null)
        {
            var query = new CheckCommunitySlugExistsQuery
            {
                Slug = slug,
                ExcludeId = excludeId
            };

            var exists = await _mediator.Send(query);
            return Ok(new { exists });
        }

        // GET: api/communities/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetCommunityStats([FromQuery] int? categoryId = null)
        {
            var query = new GetCommunityStatsQuery { CategoryId = categoryId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        #endregion

        #region Command Endpoints

        // POST: api/communities
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCommunity([FromBody] CreateCommunityDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check limit first
            var limitQuery = new CheckUserCommunityLimitQuery { UserId = userId };
            var limitResult = await _mediator.Send(limitQuery);

            if (!limitResult.CanCreateMore)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"You can only create up to {limitResult.MaxAllowed} communities",
                    errors = new[] { $"Community limit reached ({limitResult.CurrentCount}/{limitResult.MaxAllowed})" }
                });
            }

            var command = new CreateCommunityCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CoverImageUrl = dto.CoverImageUrl,
                Rules = dto.Rules,
                CategoryId = dto.CategoryId,
                IsPrivate = dto.IsPrivate,
                RequireApproval = dto.RequireApproval,
                CreatorId = userId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetCommunity),
                new { id = result.CommunityId },
                result);
        }

        // PUT: api/communities/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCommunity(int id, [FromBody] UpdateCommunityDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new UpdateCommunityCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CoverImageUrl = dto.CoverImageUrl,
                Rules = dto.Rules,
                CategoryId = dto.CategoryId,
                IsPrivate = dto.IsPrivate,
                RequireApproval = dto.RequireApproval,
                IsActive = dto.IsActive,
                CurrentUserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/communities/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCommunity(int id, [FromQuery] bool permanent = false)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new DeleteCommunityCommand
            {
                Id = id,
                CurrentUserId = userId,
                PermanentDelete = permanent
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST: api/communities/{id}/restore
        [HttpPost("{id}/restore")]
        [Authorize]
        public async Task<IActionResult> RestoreCommunity(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new RestoreCommunityCommand
            {
                Id = id,
                CurrentUserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PUT: api/communities/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize]
        public async Task<IActionResult> ToggleCommunityStatus(int id, [FromBody] bool isActive)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new ToggleCommunityStatusCommand
            {
                Id = id,
                IsActive = isActive,
                CurrentUserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion
    }
}