using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
    {
        public void Configure(EntityTypeBuilder<CartItemEntity> builder)
        {
            builder.HasKey(ci => ci.Id);

            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.InventoryItem)
                   .WithMany()
                   .HasForeignKey(ci => ci.InventoryItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
