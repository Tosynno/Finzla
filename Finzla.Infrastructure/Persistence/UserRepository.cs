using Finzla.Application.Contracts.Services;
using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class UserRepository
        : BaseRepository<AppDbContext, AppUser>, IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db) : base(db) => _db = db;

        public async Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
            => await _db.AppUsers.FirstOrDefaultAsync(u => u.Username == username, ct);

        public async Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct = default)
            => await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<bool> ExistsAsync(string username, string email, CancellationToken ct = default)
            => await _db.AppUsers.AnyAsync(
                u => u.Username == username.ToLowerInvariant()
                  || u.Email    == email.ToLowerInvariant(), ct);
    }
}
