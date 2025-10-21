using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class ComparisonListItemEntityConfiguration : IEntityTypeConfiguration<ComparisonListItemEntity>
{
    public void Configure(EntityTypeBuilder<ComparisonListItemEntity> builder)
    {
        builder.ToTable("ComparisonListItems");

        builder.HasKey(cli => cli.Id);

        builder.Property(cli => cli.InventoryItemId)
            .IsRequired();

        builder.Property(cli => cli.ComparisonListId)
            .IsRequired();

        builder.Property(cli => cli.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(cli => cli.InventoryItem)
            .WithMany()
            .HasForeignKey(cli => cli.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cli => cli.ComparisonList)
            .WithMany(cl => cl.ComparisonListItems)
            .HasForeignKey(cli => cli.ComparisonListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}