using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ShopName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.StaffName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Role)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(r => r.ReviewText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(2000);

        builder.Property(r => r.DateSubmitted)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(r => r.Account)
               .WithMany(a => a.Reviews)
               .HasForeignKey(r => r.AccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
