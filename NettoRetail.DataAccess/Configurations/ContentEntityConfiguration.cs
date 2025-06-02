using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class ContentEntityConfiguration : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        builder.ToTable("Contents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SectionType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Content);

        builder.Property(x => x.UpdatedBy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastUpdated)
            .IsRequired();

        builder.Property(x => x.IsVisible)
            .IsRequired();

        builder.HasIndex(x => new { x.SectionType, x.ShopId })
            .IsUnique();
    }
}
