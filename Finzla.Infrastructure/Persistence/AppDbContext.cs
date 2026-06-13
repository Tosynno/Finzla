using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Infrastructure.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Transaction>    Transactions    => Set<Transaction>();
        public DbSet<AccountSummary> AccountSummaries => Set<AccountSummary>();
        public DbSet<AppUser>        AppUsers         => Set<AppUser>();
        public DbSet<AuditLog>       AuditLogs        => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
