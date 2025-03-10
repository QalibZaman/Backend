using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<StaffEntity>
{
    public void Configure(EntityTypeBuilder<StaffEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(30);

        builder.Property(p => p.LastName).HasMaxLength(30);

        builder.Property(p => p.Email).HasMaxLength(256);

        builder.Property(p => p.PasswordHash).HasMaxLength(100);

        builder.Property(p => p.RoleId);

        builder.HasOne(s => s.Role)
            .WithMany(r => r.StaffEntities)
            .HasForeignKey(c => c.RoleId)
            .IsRequired(false);
    }
}