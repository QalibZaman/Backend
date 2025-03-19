using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(50);

        builder.Property(p => p.URL)
            .HasMaxLength(50);

        builder.Property(p => p.Visibility);

        builder.Property(p => p.ParentId)
            .IsRequired(false);

        builder.HasOne(p => p.ParentCategory)
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Shop)
            .WithMany(p => p.Categories)
            .HasForeignKey(p => p.ShopId);
    }
}