using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.DataAccess.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<FeedbackEntity>
{
  public void Configure(EntityTypeBuilder<FeedbackEntity> builder)
  {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Rating)
            .IsRequired(true);

        builder.Property(f => f.Title)
            .HasMaxLength(60);

        builder.Property(f => f.Text)
            .HasMaxLength(500);

        builder.Property(f => f.StaffName)
            .HasMaxLength(30)
            .HasDefaultValue("Anonymous");

        builder.Property(f => f.Role)
            .HasMaxLength(50);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasDefaultValue(FeedbackStatusEnum.Pending);

        builder.Property(f => f.DateSubmitted)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(f => f.Shop)
            .WithMany(s => s.Feedbacks)
            .HasForeignKey(f => f.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
  }
}
