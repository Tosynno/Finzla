using Finzla.Domain.Enums;
using Finzla.Domain.Errors;

namespace Finzla.Domain.Entities
{
    public sealed class Transaction
    {
        private Transaction() { }

        public Guid Id { get; private set; }
        public string ExternalId { get; private set; } = default!;
        public string AccountId { get; private set; } = default!;
        public string Currency { get; private set; } = default!;
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public string TraceId { get; set; }

        public static Result<Transaction> Create(
            string TraceId,
            string externalId,
            string accountId,
            string currency,
            decimal amount,
            string type,
            DateTime occurredAt)
        {
            if (string.IsNullOrWhiteSpace(TraceId))
                return Result<Transaction>.Failure(DomainError.Transaction.MissingTraceId);
            if (string.IsNullOrWhiteSpace(externalId))
                return Result<Transaction>.Failure(DomainError.Transaction.MissingExternalId);

            if (amount <= 0)
                return Result<Transaction>.Failure(DomainError.Transaction.InvalidAmount);

            if (!Enum.TryParse<TransactionType>(type, ignoreCase: true, out var txType))
                return Result<Transaction>.Failure(DomainError.Transaction.InvalidType);

            return Result<Transaction>.Success(new Transaction
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId.Trim(),
                AccountId = accountId.Trim(),
                Currency = currency.Trim().ToUpperInvariant(),
                Amount = amount,
                Type = txType,
                OccurredAt = occurredAt,
                ReceivedAt = DateTime.UtcNow
            });
        }
    }

}
