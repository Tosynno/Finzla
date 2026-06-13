using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Finzla.Application.Features
{
    public sealed class AccountSummaryService(
        IAccountSummaryRepository summaryRepo,
        ILogger<AccountSummaryService> logger)
    {
        public async Task<Result<AccountSummaryDto>> GetAsync(
            string accountId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return Result<AccountSummaryDto>.Failure(DomainError.Account.InvalidAccountId);

            var summary = await summaryRepo.FindAsync(accountId.Trim(), cancellationToken);

            if (summary is null)
            {
                logger.LogInformation("Account summary not found for {AccountId}", accountId);
                return Result<AccountSummaryDto>.Failure(DomainError.Account.NotFound);
            }

            logger.LogInformation("Account summary retrieved for {AccountId}", accountId);

            return Result<AccountSummaryDto>.Success(new AccountSummaryDto(
                summary.AccountId,
                summary.Balance,
                summary.TotalCredits,
                summary.TotalDebits,
                summary.TotalTransactions,
                summary.LastUpdatedAt));
        }
    }
}
