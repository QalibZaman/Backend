using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class ComparisonListEntityConfiguration : IEntityTypeConfiguration<ComparisonListEntity>
    {
        public void Configure(EntityTypeBuilder<ComparisonListEntity> builder)
        {
            builder.ToTable("comparison_lists");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(x => x.CategoryId)
                   .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.HasIndex(x => new { x.UserId, x.CategoryId })
                   .IsUnique();

            builder.HasMany(x => x.ComparisonListItems)
                   .WithOne(x => x.ComparisonList)
                   .HasForeignKey(x => x.ComparisonListId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
