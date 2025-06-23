using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class FeatureEntityConfiguration : IEntityTypeConfiguration<FeatureEntity>
    {
        public void Configure(EntityTypeBuilder<FeatureEntity> builder)
        {
            builder.ToTable("Features");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(f => f.Description)
                .HasMaxLength(500);

            builder
                .HasMany(f => f.SubscriptionPlans)
                .WithMany(p => p.Features)
                .UsingEntity(j => j.ToTable("FeatureSubscriptionPlans")); // Имя промежуточной таблицы
        }
    }
}
