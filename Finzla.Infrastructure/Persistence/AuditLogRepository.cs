using Finzla.Application.Contracts.Services;
using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class AuditLogRepository
        : BaseRepository<AppDbContext, AuditLog>, IAuditLogRepository
    {
        private readonly AppDbContext _db;

        public AuditLogRepository(AppDbContext db) : base(db) => _db = db;

        public async Task<IReadOnlyList<AuditLog>> GetByUsernameAsync(
            string username, int page = 1, int pageSize = 50, CancellationToken ct = default)
            => await _db.AuditLogs
                .Where(a => a.Username == username)
                .OrderByDescending(a => a.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task LogAsync(AuditLog entry, CancellationToken ct = default)
        {
            await _db.AuditLogs.AddAsync(entry, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
