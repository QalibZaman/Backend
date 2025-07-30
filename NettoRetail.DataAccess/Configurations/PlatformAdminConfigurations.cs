using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class PlatformAdminConfigurations : IEntityTypeConfiguration<PlatformAdminEntity>
{
    public void Configure(EntityTypeBuilder<PlatformAdminEntity> builder)
    {
        builder.HasKey(x => x.AccountId);

        builder.HasOne(x => x.Account)
            .WithOne()
            .HasForeignKey<PlatformAdminEntity>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.PlatformAdminStatus).HasConversion<string>();
    }
}
