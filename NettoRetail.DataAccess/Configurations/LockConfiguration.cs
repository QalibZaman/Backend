using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class LockConfiguration : IEntityTypeConfiguration<EntityLock>
{
    public void Configure(EntityTypeBuilder<EntityLock> builder)
    {
        // Composite key
        builder.HasKey(x => new { x.ResourceType, x.ResourceId, x.ShopId });

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ResourceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ShopId)
            .IsRequired();

        builder.Property(x => x.LockedByUserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.LockedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(x => x.LockedByUser)
               .WithMany(x => x.Locks)
               .HasForeignKey(x => x.LockedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId, x.ShopId })
           .IsUnique()
           .HasDatabaseName("UX_EntityLocks_Resource");
    }
}
