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

        builder.HasOne(s => s.Staff)
            .WithMany(s => s.Sessions)
            .HasForeignKey(s => s.StaffId);

        builder.HasOne(s => s.Account)
            .WithMany()
            .HasForeignKey(s => s.AccountId);
    }
}