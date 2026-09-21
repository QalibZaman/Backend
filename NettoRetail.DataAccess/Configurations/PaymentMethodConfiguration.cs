using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethodEntity>
{
    public void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripePaymentMethodId)
            .IsRequired();

        builder.HasIndex(x => x.StripePaymentMethodId)
            .IsUnique();

        builder.Property(x => x.CardBrand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FourLastCardNumbers)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}