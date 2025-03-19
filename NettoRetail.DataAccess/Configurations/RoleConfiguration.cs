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

        builder.HasMany(model => model.PermissionRoles)
            .WithOne(model => model.Role)
            .HasForeignKey(model => model.RoleId);

        builder.HasMany(model => model.StaffEntities)
            .WithOne(model => model.Role)
            .HasForeignKey(model => model.RoleId);

        builder.HasOne(model => model.Shop)
            .WithMany(model => model.Roles)
            .HasForeignKey(model => model.ShopId);

        builder.HasData(
            new RoleEntity() { Id = 1, Name = "Admin", Description = "Has full access to the system" });
    }
}