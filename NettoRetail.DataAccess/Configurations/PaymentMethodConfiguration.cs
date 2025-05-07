using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethodEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.PaymentMethods)
                .HasForeignKey(x => x.AccountId)
                .IsRequired(false);

            builder.Property(pm => pm.StripePaymentMethodId)
                .IsRequired();

            builder.Property(pm => pm.FourLastCardNumbers)
                .HasMaxLength(4);

            builder.Property(pm => pm.CardBrand)
                .HasMaxLength(50);

            builder.Property(pm => pm.CardExpMonth)
                .IsRequired();

            builder.Property(pm => pm.CardExpYear)
                .IsRequired();
        }
    }
}
