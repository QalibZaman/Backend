using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<StaffEntity>
{
    public void Configure(EntityTypeBuilder<StaffEntity> builder)
    {
        builder.HasMany(a => a.Locks)
            .WithOne(l => l.LockedByUser)
            .HasForeignKey(l => l.LockedByUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
