using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class OffersProductConfiguration : IEntityTypeConfiguration<OffersProductEntity>
{
    public void Configure(EntityTypeBuilder<OffersProductEntity> builder)
    {
        builder.ToTable("OfferSectionProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OfferId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.HasOne(x => x.Offer)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}