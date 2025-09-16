using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class OnboardingSlideDraftEntityConfiguration : IEntityTypeConfiguration<OnboardingSlideDraftEntity>
{
    public void Configure(EntityTypeBuilder<OnboardingSlideDraftEntity> builder)
    {
        builder.ToTable("OnboardingSlideDrafts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SlideId)
            .IsRequired(false);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.DateSaved)
            .IsRequired();

        builder.HasIndex(x => new { x.SlideId, x.UserId })
            .IsUnique();
    }
}
