using BlogHybrid.Application.Commands.Member;
using BlogHybrid.Application.Queries.Member;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MembersController> _logger;

        public MembersController(IMediator mediator, ILogger<MembersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // GET: api/members
        [HttpGet]
        public async Task<IActionResult> GetMembers([FromQuery] GetMembersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/members/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(string id)
        {
            var query = new GetMemberByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "Member not found" });

            return Ok(result);
        }

        // POST: api/members
        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] CreateMemberCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetMember), new { id = result.MemberId }, result);
        }

        // PUT: api/members/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(string id, [FromBody] UpdateMemberCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return NoContent();
        }

        // DELETE: api/members/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var command = new DeleteMemberCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return NoContent();
        }

        // PUT: api/members/{id}/change-password
        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangeMemberPasswordCommand command)
        {
            command.MemberId = id;
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PUT: api/members/{id}/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var command = new ToggleMemberStatusCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}