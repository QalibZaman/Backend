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

        builder.Property(p => p.ProductUpc);
        builder.HasIndex(p => p.ProductUpc).IsUnique();

        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.Property(p => p.Description).HasMaxLength(500);

        builder.Property(p => p.LastUpdatedDate);

        builder.Property(p => p.IsActive);
    }
}