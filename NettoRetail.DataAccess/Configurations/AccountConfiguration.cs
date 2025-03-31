using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(30);

        builder.Property(p => p.LastName).HasMaxLength(30);

        builder.Property(p => p.Email).HasMaxLength(256);

        builder.Property(p => p.PasswordHash).HasMaxLength(100);

        builder.Property(p => p.RoleId);

        builder.HasOne(s => s.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(c => c.RoleId)
            .IsRequired(false);

        builder.Property(p => p.Status).HasConversion<string>();
    }
}