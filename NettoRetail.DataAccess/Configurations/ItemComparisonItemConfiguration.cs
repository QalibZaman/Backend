using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.DataAccess.Configurations;

public class ItemComparisonItemConfiguration : IEntityTypeConfiguration<ItemComparisonItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemComparisonItemEntity> builder)
    {
        builder.ToTable("ItemComparison_Item");

        builder.HasKey(model => new { model.ComparisonId, model.InventoryItemId });

        builder.HasOne(model => model.InventoryItem)
            .WithMany(model => model.ComparisonItems)
            .HasForeignKey(model => model.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(model => model.Comparison)
            .WithMany(model => model.ComparisonItems)
            .HasForeignKey(model => model.ComparisonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
