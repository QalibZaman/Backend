using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class ComparisonListItemEntityConfiguration : IEntityTypeConfiguration<ComparisonListItemEntity>
    {
        public void Configure(EntityTypeBuilder<ComparisonListItemEntity> builder)
        {
            builder.ToTable("comparison_list_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductName)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(x => x.Price)
                   .IsRequired()
                   .HasColumnType("numeric(10,2)");

            builder.Property(x => x.PromotionPrice)
                   .HasColumnType("numeric(10,2)");

            builder.Property(x => x.Brand)
                   .HasMaxLength(64);

            builder.Property(x => x.IsPromoted)
                   .IsRequired();

            builder.Property(x => x.AvailableAmount)
                   .IsRequired();

            builder.Property(x => x.ReservedAmount)
                   .IsRequired();

            builder.Property(x => x.OnHandAmount)
                   .IsRequired();

            builder.Property(x => x.ProductLocation)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(x => x.InventoryItemId)
                   .IsRequired();

            builder.Property(x => x.ComparisonListId)
                   .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.HasIndex(x => new { x.ComparisonListId, x.InventoryItemId })
                   .IsUnique();

            builder.HasOne(x => x.InventoryItem)
                   .WithMany()
                   .HasForeignKey(x => x.InventoryItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
