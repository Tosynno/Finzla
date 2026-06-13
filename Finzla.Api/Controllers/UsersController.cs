using Finzla.Application.Dtos;
using Finzla.Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finzla.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Authorize]
    public sealed class UsersController(UserService userService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await userService.CreateAsync(request, cancellationToken);
            if (result.IsFailure)
                return result.Error!.Code == "USR_002"
                    ? Conflict(new { error = result.Error.Code, detail = result.Error.Message })
                    : BadRequest(new { error = result.Error!.Code, detail = result.Error.Message });
            return CreatedAtAction(nameof(GetUser), new { id = result.Value!.Id }, result.Value);
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var result = await userService.GetAllAsync(cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await userService.GetByIdAsync(id, cancellationToken);
            if (result.IsFailure)
                return NotFound(new { error = result.Error!.Code, detail = result.Error.Message });
            return Ok(result.Value);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetUserStatus(
            Guid id,
            [FromQuery] bool activate,
            CancellationToken cancellationToken)
        {
            var result = await userService.ToggleActiveAsync(id, activate, cancellationToken);
            if (result.IsFailure)
                return NotFound(new { error = result.Error!.Code, detail = result.Error.Message });
            return Ok(result.Value);
        }
    }
}
