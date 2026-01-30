using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Name)
            .HasMaxLength(50);

        builder.Property(model => model.Description)
            .HasMaxLength(500);

        builder.Property(model => model.LockedAtUtc);

        builder.Property(model => model.LockedByUserId);

        builder.HasMany(model => model.PermissionRoles)
            .WithOne(model => model.Role)
            .HasForeignKey(model => model.RoleId);

        builder.HasOne(model => model.Shop)
            .WithMany(model => model.Roles)
            .HasForeignKey(model => model.ShopId);

        builder.HasMany(r => r.Accounts)
            .WithOne(s => s.Role)
            .HasForeignKey(c => c.RoleId);

        builder.HasOne(r => r.LockedByUser)
            .WithMany(a => a.LockedRoles)
            .HasForeignKey(r => r.LockedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasData(
            new RoleEntity() { Id = 1, Name = "Admin", Description = "Has full access to the system" },
            new RoleEntity() { Id = 2, Name = "Customer", Description = "Has access to only what the customer can do" },
            new RoleEntity() { Id = 99, Name = "SuperAdmin", Description = "System-level admin from RetailTech with full access across tenants" });
    }
}