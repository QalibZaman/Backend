using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlanEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanEntity> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Name)
            .HasMaxLength(25);

        builder.Property(model => model.Moto)
            .HasMaxLength(70);

        builder.Property(model => model.Description)
            .HasMaxLength(500);

        builder.HasMany(model => model.Shops)
            .WithOne(model => model.SubscriptionPlan)
            .HasForeignKey(model => model.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
