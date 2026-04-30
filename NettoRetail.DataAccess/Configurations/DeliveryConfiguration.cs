using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<DeliveryEntity>
{
    public void Configure(EntityTypeBuilder<DeliveryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeliveryType)
            .HasConversion<string>();

        builder.Property(x => x.DeliveryFee)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaymentTotal)
            .HasPrecision(18, 2);
        builder.HasIndex(x => x.CustomerId).IsUnique();
        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithOne(x => x.Delivery)
            .HasForeignKey<DeliveryEntity>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Delivery)
            .HasForeignKey<DeliveryEntity>(x => x.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Address)
            .WithMany(x => x.Deliveries)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
