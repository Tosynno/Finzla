using Finzla.Domain.Entities;

namespace Finzla.Application.Contracts.Services
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IReadOnlyList<AuditLog>> GetByUsernameAsync(
            string username,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default);
    }
}
