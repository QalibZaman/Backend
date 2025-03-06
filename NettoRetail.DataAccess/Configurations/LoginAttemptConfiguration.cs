using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttemptEntity>
{
    public void Configure(EntityTypeBuilder<LoginAttemptEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FailedAttempts).IsRequired();

        builder.Property(p => p.LockoutEndTime).IsRequired(false);

        builder.Property(p => p.LastFailedLoginTime).IsRequired();

        builder.HasOne(s => s.Staff)
            .WithOne(s => s.LoginAttempt)
            .HasForeignKey<LoginAttemptEntity>(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}