using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class NewProductsSectionConfiguration : IEntityTypeConfiguration<NewProductsSectionEntity>
{
    public void Configure(EntityTypeBuilder<NewProductsSectionEntity> builder)
    {
        builder.ToTable("NewProductsSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShopId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.IsVisible).IsRequired();
        builder.Property(x => x.SelectionType)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.LastUpdated).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Section)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
