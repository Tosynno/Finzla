using Finzla.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finzla.Infrastructure.Persistence.Configurations
{
    public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AppUsers");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                   .IsRequired().HasMaxLength(50);
            builder.HasIndex(u => u.Username)
                   .IsUnique().HasDatabaseName("IX_AppUsers_Username");

            builder.Property(u => u.Email)
                   .IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email)
                   .IsUnique().HasDatabaseName("IX_AppUsers_Email");

            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);
            builder.Property(u => u.IsActive).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();
        }
    }
}
