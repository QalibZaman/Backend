using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class InventoryItemEntityConfiguration : IEntityTypeConfiguration<InventoryItemEntity>
{
public void Configure(EntityTypeBuilder<InventoryItemEntity> builder)
{
    builder.HasKey(i => i.Id);

    builder.Property(i => i.ProductName)
        .IsRequired()
        .HasMaxLength(150)
        .HasColumnType("character varying(50)");

    builder.Property(i => i.ProductUpc)
        .IsRequired()
        .HasMaxLength(12)
        .HasColumnType("character varying(12)");

    builder.Property(i => i.Description)
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(i => i.ProductLocation)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(i => i.Price).IsRequired();
    builder.Property(i => i.CategoryId).IsRequired();
    builder.Property(i => i.LastUpdatedDate).IsRequired();
    builder.Property(i => i.IsActive).IsRequired();
    builder.Property(i => i.Status).IsRequired();
    builder.Property(i => i.AvailableAmount).IsRequired();
    builder.Property(i => i.ReservedAmount).IsRequired();
    builder.Property(i => i.OnHandAmount).IsRequired();
    builder.Property(i => i.ShopId).IsRequired();
    builder.Property(i => i.BrandId).IsRequired(false);

    builder
        .HasMany(i => i.Images)
        .WithOne(img => img.InventoryItem)
        .HasForeignKey(img => img.InventoryItemId)
        .OnDelete(DeleteBehavior.Cascade);
}
}
