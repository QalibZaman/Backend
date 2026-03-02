using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;
public class OrderStatusHistoryEntityConfiguration : IEntityTypeConfiguration<OrderStatusHistoryEntity>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistoryEntity> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne(o => o.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(osh => osh.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        builder.HasOne(o => o.UpdatedBy)
            .WithMany()
            .HasForeignKey(o => o.UpdatedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
