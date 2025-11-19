using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;
public class RecentlyViewedProductConfiguration : IEntityTypeConfiguration<RecentlyViewedProductEntity>
{
    public void Configure(EntityTypeBuilder<RecentlyViewedProductEntity> builder)
    {
        builder.HasKey(rv => new { rv.AccountId, rv.InventoryItemId });

        builder.HasOne(rv => rv.Account)
               .WithMany(a => a.RecentlyViewedProducts)
               .HasForeignKey(rv => rv.AccountId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rv => rv.InventoryItem)
               .WithMany(ii => ii.RecentlyViewedByUsers)
               .HasForeignKey(rv => rv.InventoryItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(rv => rv.ViewedAtUtc)
               .IsRequired();
    }
}
