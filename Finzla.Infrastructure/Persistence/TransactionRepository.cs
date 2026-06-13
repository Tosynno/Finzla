using Finzla.Application.Contracts.Services;
using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class TransactionRepository
        : BaseRepository<AppDbContext, Transaction>, ITransactionRepository
    {
        private readonly AppDbContext _db;

        public TransactionRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(string TraceId, CancellationToken cancellationToken = default)
            => await _db.Transactions.AnyAsync(x => x.TraceId == TraceId, cancellationToken);
    }
}
