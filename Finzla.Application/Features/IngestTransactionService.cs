using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Entities;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Finzla.Application.Features
{
    public sealed class IngestTransactionService(
        ITransactionRepository transactionRepo,
        IAccountSummaryRepository summaryRepo,
        ILogger<IngestTransactionService> logger)
    {
        public async Task<Result<IngestTransactionResponse>> ExecuteAsync(
            IngestTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await transactionRepo.ExistsAsync(request.TraceId, cancellationToken))
            {
                logger.LogInformation("Duplicate transaction received: {ExternalId}", request.ExternalId);

                var existingSummary = await summaryRepo.FindAsync(request.AccountId, cancellationToken)
                                      ?? AccountSummary.Init(request.AccountId);

                return Result<IngestTransactionResponse>.Success(
                    ToResponse(request.TraceId, "Duplicate", existingSummary));
            }

            var txResult = Transaction.Create(
                request.TraceId,
                request.ExternalId,
                request.AccountId,
                request.Currency,
                request.Amount,
                request.Type,
                request.OccurredAt);

            if (txResult.IsFailure)
                return Result<IngestTransactionResponse>.Failure(txResult.Error!);

            var transaction = txResult.Value!;

            
            var summary = await summaryRepo.FindAsync(request.AccountId, cancellationToken);
            bool isNew = summary is null;

            summary ??= AccountSummary.Init(request.AccountId);
            summary.Apply(transaction);

            await transactionRepo.AddAsync(transaction, cancellationToken);

            if (isNew)
                await summaryRepo.AddAsync(summary, cancellationToken);
            else
                await summaryRepo.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Transaction {ExternalId} accepted. Account {AccountId} balance: {Balance}",
                transaction.ExternalId, transaction.AccountId, summary.Balance);

            return Result<IngestTransactionResponse>.Success(
                ToResponse(transaction.ExternalId, "Accepted", summary));
        }

        private static IngestTransactionResponse ToResponse(
            string externalId, string status, AccountSummary summary) =>
            new(
                externalId,
                status,
                new AccountSummaryDto(
                    summary.AccountId,
                    summary.Balance,
                    summary.TotalCredits,
                    summary.TotalDebits,
                    summary.TotalTransactions,
                    summary.LastUpdatedAt));
    }
}
