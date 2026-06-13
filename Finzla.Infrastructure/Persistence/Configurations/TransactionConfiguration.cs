using Finzla.Domain.Entities;
using Finzla.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finzla.Infrastructure.Persistence.Configurations
{
    public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ExternalId)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(t => t.ExternalId)
                   .IsUnique()
                   .HasDatabaseName("IX_Transactions_ExternalId");

            builder.HasIndex(t => t.AccountId)
                   .HasDatabaseName("IX_Transactions_AccountId");

            builder.Property(t => t.AccountId)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(t => t.Currency)
                   .IsRequired()
                   .HasMaxLength(8);

            builder.Property(t => t.Amount)
                   .HasPrecision(18, 4);

            builder.Property(t => t.Type)
                   .HasConversion(
                       v => v.ToString(),
                       v => Enum.Parse<TransactionType>(v))
                   .HasMaxLength(16);

            builder.Property(t => t.OccurredAt).IsRequired();
            builder.Property(t => t.ReceivedAt).IsRequired();
        }
    }

}
