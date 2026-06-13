using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finzla.Infrastructure.Persistence.Configurations
{
    public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).UseIdentityAlwaysColumn();

            builder.Property(a => a.Username).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Resource).IsRequired().HasMaxLength(512);
            builder.Property(a => a.HttpMethod).IsRequired().HasMaxLength(10);
            builder.Property(a => a.IpAddress).HasMaxLength(45);
            builder.Property(a => a.UserAgent).HasMaxLength(512);
            builder.Property(a => a.RequestBody).HasMaxLength(4000);
            builder.Property(a => a.ErrorMessage).HasMaxLength(2000);

            builder.HasIndex(a => a.Username).HasDatabaseName("IX_AuditLogs_Username");
            builder.HasIndex(a => a.OccurredAt).HasDatabaseName("IX_AuditLogs_OccurredAt");
        }
    }
}
