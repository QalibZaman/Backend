using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderStatus)
            .HasConversion<string>();

        builder.HasMany(p => p.OrderItems)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId);

        builder.Property(p => p.Creator)
            .HasMaxLength(200);
    }
}
