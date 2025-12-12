using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

internal class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistoryEntity>
{
    public void Configure(EntityTypeBuilder<PaymentHistoryEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AmountCharged).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PlanName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.BillingPeriod).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);

        builder.HasIndex(e => e.TransactionId).IsUnique();

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}