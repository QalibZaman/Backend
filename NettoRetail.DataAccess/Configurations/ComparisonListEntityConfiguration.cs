using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class ComparisonListEntityConfiguration : IEntityTypeConfiguration<ComparisonListEntity>
{
    public void Configure(EntityTypeBuilder<ComparisonListEntity> builder)
    {
        builder.ToTable("ComparisonLists");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.CategoryId)
            .IsRequired();

        builder.Property(cl => cl.CustomerId)
            .IsRequired();

        builder.Property(cl => cl.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(cl => cl.Customer)
            .WithMany()
            .HasForeignKey(cl => cl.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cl => cl.ComparisonListItems)
            .WithOne(cli => cli.ComparisonList)
            .HasForeignKey(cli => cli.ComparisonListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}