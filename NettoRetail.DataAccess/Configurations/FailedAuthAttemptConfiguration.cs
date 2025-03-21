using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class FailedAuthAttemptConfiguration : IEntityTypeConfiguration<FailedAuthAttemptEntity>
{
    public void Configure(EntityTypeBuilder<FailedAuthAttemptEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FailedAttempts).IsRequired();

        builder.Property(p => p.LockoutEndTime).IsRequired(false);

        builder.Property(p => p.LastFailedTime).IsRequired();

        builder.HasOne(s => s.Account)
            .WithOne(s => s.FailedAuthAttempt)
            .HasForeignKey<FailedAuthAttemptEntity>(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}