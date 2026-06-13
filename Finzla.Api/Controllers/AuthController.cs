using Finzla.Application.Dtos;
using Finzla.Application.Features;
using Microsoft.AspNetCore.Mvc;

namespace Finzla.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public sealed class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromHeader(Name = "AppSecurityKey")] string? appSecurityKey,
            CancellationToken cancellationToken)
        {
            var ip     = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await authService.LoginAsync(
                request, appSecurityKey ?? string.Empty, ip, cancellationToken);

            if (result.IsFailure)
            {
                var err = result.Error!;
                return err.Code is "AUTH_002" or "AUTH_003" or "AUTH_004"
                    ? Unauthorized(new { error = err.Code, detail = err.Message })
                    : BadRequest(new { error = err.Code, detail = err.Message });
            }

            return Ok(result.Value);
        }
    }
}
