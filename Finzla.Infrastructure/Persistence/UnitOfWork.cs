using Finzla.Application.Contracts.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private readonly AppDbContext _context = context;
        private IDbContextTransaction? _transaction;

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            _transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                await _transaction.RollbackAsync(cancellationToken);
        }
    }
}
