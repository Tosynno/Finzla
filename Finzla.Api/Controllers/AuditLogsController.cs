using Finzla.Application.Dtos;
using Finzla.Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finzla.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Produces("application/json")]
    [Authorize]
    public sealed class AuditLogsController(AuditLogService auditService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await auditService.GetAllAsync(page, pageSize, cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("{username}")]
        [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUser(
            string username,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await auditService.GetByUsernameAsync(username, page, pageSize, cancellationToken);
            if (result.IsFailure)
                return NotFound(new { error = result.Error!.Code, detail = result.Error.Message });
            return Ok(result.Value);
        }
    }
}
