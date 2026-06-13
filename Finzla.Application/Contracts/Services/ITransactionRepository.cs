using Finzla.Domain.Entities;

namespace Finzla.Application.Contracts.Services
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<bool> ExistsAsync(string TraceId, CancellationToken cancellationToken = default);
    }
}
