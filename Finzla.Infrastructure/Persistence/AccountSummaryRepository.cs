using Finzla.Application.Contracts.Services;
using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class AccountSummaryRepository
        : BaseRepository<AppDbContext, AccountSummary>, IAccountSummaryRepository
    {
        private readonly AppDbContext _db;

        public AccountSummaryRepository(AppDbContext db) : base(db) => _db = db;

        public async Task<AccountSummary?> FindAsync(
            string accountId, CancellationToken ct = default)
            => await _db.AccountSummaries
                        .FirstOrDefaultAsync(a => a.AccountId == accountId, ct);
    }
}
