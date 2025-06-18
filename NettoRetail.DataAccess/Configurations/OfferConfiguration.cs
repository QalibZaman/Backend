using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<OfferEntity>
{
    public void Configure(EntityTypeBuilder<OfferEntity> builder)
    {
        builder.ToTable("OfferSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShopId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Preview).HasMaxLength(1000);
        builder.Property(x => x.IsVisible).IsRequired();
        builder.Property(x => x.SelectionType)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.LastUpdated).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Offer)
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
