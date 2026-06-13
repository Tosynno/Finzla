using Finzla.Domain.Enums;

namespace Finzla.Domain.Entities
{
    public sealed class AccountSummary
    {
        private AccountSummary() { }

        public string AccountId { get; private set; } = default!;
        public decimal Balance { get; private set; }
        public int TotalTransactions { get; private set; }
        public decimal TotalCredits { get; private set; }
        public decimal TotalDebits { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }

        public static AccountSummary Init(string accountId) => new()
        {
            AccountId = accountId,
            Balance = 0m,
            LastUpdatedAt = DateTime.UtcNow
        };

        public void Apply(Transaction transaction)
        {
            if (transaction.Type == TransactionType.Credit)
            {
                Balance += transaction.Amount;
                TotalCredits += transaction.Amount;
            }
            else
            {
                Balance -= transaction.Amount;
                TotalDebits += transaction.Amount;
            }

            TotalTransactions++;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

}
