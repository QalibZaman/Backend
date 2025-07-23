using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.DataAccess.Configurations;

public class NewProductsProductConfiguration : IEntityTypeConfiguration<NewProductsProductEntity>
{
  public void Configure(EntityTypeBuilder<NewProductsProductEntity> builder)
  {
    builder.ToTable("NewProductsSectionProducts");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.SectionId).IsRequired();
    builder.Property(x => x.InventoryItemId).IsRequired();
    builder.Property(x => x.DisplayOrder).IsRequired();

    builder.HasOne(x => x.Section)
        .WithMany(x => x.Products)
        .HasForeignKey(x => x.SectionId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.InventoryItem)
        .WithMany()
        .HasForeignKey(x => x.InventoryItemId)
        .OnDelete(DeleteBehavior.Restrict);
  }
}
