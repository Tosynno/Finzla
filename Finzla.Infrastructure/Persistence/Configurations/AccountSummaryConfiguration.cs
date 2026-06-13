using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Finzla.Infrastructure.Persistence.Configurations
{
    public sealed class AccountSummaryConfiguration : IEntityTypeConfiguration<AccountSummary>
    {
        public void Configure(EntityTypeBuilder<AccountSummary> builder)
        {
            builder.ToTable("AccountSummaries");

            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.AccountId)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(a => a.Balance).HasPrecision(18, 4);
            builder.Property(a => a.TotalCredits).HasPrecision(18, 4);
            builder.Property(a => a.TotalDebits).HasPrecision(18, 4);
            builder.Property(a => a.LastUpdatedAt).IsRequired();
        }
    }
}
