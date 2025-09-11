using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class OnboardingSlideEntityConfiguration : IEntityTypeConfiguration<OnboardingSlideEntity>
{
    public void Configure(EntityTypeBuilder<OnboardingSlideEntity> builder)
    {
        builder.ToTable("OnboardingSlides");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.IsVisible)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .IsRequired();

        builder.HasIndex(x => x.Order)
            .IsUnique(false);
    }
}
