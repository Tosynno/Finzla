using Finzla.Domain.Entities;

namespace Finzla.Application.Contracts.Services
{
    
    public interface IAccountSummaryRepository : IRepository<AccountSummary>
    {
        Task<AccountSummary?> FindAsync(string accountId, CancellationToken cancellationToken = default);
    }
}
