using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItemEntity>
{
    public void Configure(EntityTypeBuilder<InventoryItemEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductName).HasMaxLength(50);

        builder.HasOne(p => p.Category)
            .WithMany(p => p.InventoryItems)
            .HasForeignKey(p => p.CategoryId);

        builder.HasOne(p => p.Shop)
            .WithMany(s => s.InventoryItems)
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(true);

        builder.Property(p => p.PromotionPrice)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(p => p.ProductUpc);
        builder.HasIndex(p => p.ProductUpc).IsUnique();

        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.Property(p => p.Description).HasMaxLength(500);

        builder.Property(p => p.ProductLocation).HasMaxLength(100);

        builder.Property(p => p.Status).HasConversion<string>();
    }
}