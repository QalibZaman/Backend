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

        builder.Property(f => f.StaffName)
            .HasMaxLength(60);

        builder.Property(f => f.RejectionReason)
            .HasMaxLength(500);

        builder.Property(f => f.ShopName)
            .HasMaxLength(90);

        builder.Property(f => f.Text)
            .HasMaxLength(500);

        builder.Property(f => f.IsPublic)
            .IsRequired();

        builder.Property(f => f.IsVisible)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasDefaultValue(FeedbackStatusEnum.Pending);

        builder.Property(f => f.DateSubmitted)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(f => f.Account)
            .WithMany()
            .HasForeignKey(f => f.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
  }
}
