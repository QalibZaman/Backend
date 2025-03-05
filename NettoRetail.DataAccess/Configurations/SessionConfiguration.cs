using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Jwt);

        builder.HasIndex(s => s.Jwt)
            .IsUnique();

        builder.Property(s => s.RefreshToken);

        builder.HasIndex(s => s.RefreshToken)
            .IsUnique();

        builder.Property(s => s.Device)
            .HasMaxLength(50);

        builder.Property(s => s.Browser)
            .HasMaxLength(50);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45);

        builder.Property(s => s.Location)
            .HasMaxLength(100);

        builder.HasOne<StaffEntity>(s => s.Account)
            .WithMany(r => r.Sessions)
            .HasForeignKey(c => c.AccountId);

        // builder.ToTable("Session");
    }
}