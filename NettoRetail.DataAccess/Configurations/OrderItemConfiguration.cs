using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.DataAccess.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
    {
        public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
        {
            builder.ToTable("Order_Item");

            builder.HasKey(p => p.Id);

            builder.HasIndex(p => new { p.OrderId, p.InventoryItemId })
                .IsUnique();

            builder.HasOne(p => p.Order)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(p => p.OrderId);

            builder.HasOne(p => p.InventoryItem)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(p => p.InventoryItemId);
        }
    }
}