using Finzla.Application.Dtos;
using Finzla.Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finzla.Api.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [Produces("application/json")]
    [Authorize]
    public sealed class TransactionsController(
        IngestTransactionService ingestService,
        AccountSummaryService summaryService) : ControllerBase
    {
        [HttpPost("transactions")]
        [ProducesResponseType<IngestTransactionResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<IngestTransactionResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IngestTransaction(
            [FromBody] IngestTransactionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await ingestService.ExecuteAsync(request, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error!.Code, detail = result.Error.Message });

            return result.Value!.Status == "Accepted"
                ? StatusCode(StatusCodes.Status201Created, result.Value)
                : Ok(result.Value);
        }

        [HttpGet("accounts/{accountId}/summary")]
        [ProducesResponseType<AccountSummaryDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccountSummary(
            string accountId,
            CancellationToken cancellationToken)
        {
            var result = await summaryService.GetAsync(accountId, cancellationToken);

            if (result.IsFailure)
                return NotFound(new { error = result.Error!.Code, detail = result.Error.Message });

            return Ok(result.Value);
        }
    }
}
