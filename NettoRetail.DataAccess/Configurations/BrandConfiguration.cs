using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<BrandEntity>
{
    public void Configure(EntityTypeBuilder<BrandEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(50);
        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(100);
        builder.HasIndex(x => x.WebsiteUrl)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.Shop)
            .WithMany(s => s.Brands)
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
