using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReviewEntity>
    {
        public void Configure(EntityTypeBuilder<ProductReviewEntity> builder)
        { 
                builder.ToTable("ProductReviews");
                builder.HasKey(r => r.Id);

                builder.Property(r => r.ProductUpc)
                    .IsRequired();

                builder.Property(r => r.Rating)
                    .IsRequired();

                builder.Property(r => r.Title)
                    .IsRequired();

                builder.Property(r => r.ReviewText)
                    .IsRequired(false);

                builder.Property(r => r.VariantInfo)
                    .HasColumnType("text");

                builder.Property(r => r.PhotoUrls)
                    .HasColumnType("jsonb")
                    .IsRequired(false);

                builder.Property(r => r.DateSubmitted)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                builder.Property(r => r.ReviewStatus)
                    .IsRequired()
                    .HasConversion<string>();

                builder.HasIndex(r => r.ProductUpc)
                    .IsUnique();

                builder.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ProductReviews_Accounts_CustomerId");

                builder.HasOne(r => r.InventoryItem)
                    .WithMany()
                    .HasForeignKey(r => r.InventoryItemId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
