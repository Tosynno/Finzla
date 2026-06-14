using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Entities;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Finzla.Application.Features
{
    public sealed class IngestTransactionService(
        ITransactionRepository transactionRepo,
        IAccountSummaryRepository summaryRepo,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<IngestTransactionService> logger)
    {
        private readonly string _defaultGLAccount = configuration["Auth:GLAccount"] ?? "2098509840";
        public async Task<Result<IngestTransactionResponse>> ExecuteAsync(
            IngestTransactionRequest request,
            CancellationToken cancellationToken = default)
        {

            if (await transactionRepo.ExistsAsync(request.TraceId, cancellationToken))
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Duplicate transaction received: {ExternalId}", request.DebitAccount);

                var existingSummary = await summaryRepo.FindAsync(request.AccountId, cancellationToken)
                                      ?? AccountSummary.Init(request.AccountId);

                return Result<IngestTransactionResponse>.Success(
                    ToResponse(request.TraceId, "Duplicate", existingSummary));
            }

            string debitAccount = request.DebitAccount ?? string.Empty;
            string accountId = request.AccountId;

            if (request.IsOtherBank)
            {
                switch (request.Type?.ToUpperInvariant())
                {
                    case "CREDIT":
                        // Other Bank Credit → GLAccount becomes the debit leg reference
                        debitAccount = _defaultGLAccount;
                        break;

                    case "DEBIT":
                        // Other Bank Debit → GLAccount is used as the target (Finzla side)
                        accountId = _defaultGLAccount;
                        break;

                    default:
                        // Fallback - use original values
                        break;
                }
            }

            var txResult = Transaction.Create(
                request.TraceId,
                debitAccount,
                accountId,
                request.Currency,
                request.Amount,
                request.Type!,
                request.OccurredAt);

            if (txResult.IsFailure)
                return Result<IngestTransactionResponse>.Failure(txResult.Error!);

            var transaction = txResult.Value!;

            
            var summary = await summaryRepo.FindAsync(request.AccountId, cancellationToken);
            bool isNew = summary is null;

            summary ??= AccountSummary.Init(request.AccountId);
            summary.Apply(transaction);


            try
            {
                await unitOfWork.BeginTransactionAsync(cancellationToken);

                await transactionRepo.AddAsync(transaction, cancellationToken);

                if (isNew)
                    await summaryRepo.AddAsync(summary, cancellationToken);
                else
                    await summaryRepo.SaveChangesAsync(cancellationToken);

                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            if (logger.IsEnabled(LogLevel.Information))
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
