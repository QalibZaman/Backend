using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Title)
            .HasMaxLength(50);

        builder.HasMany(model => model.PermissionRoles)
            .WithOne(model => model.Permission)
            .HasForeignKey(model => model.PermissionId);

        builder.HasData(
            new PermissionEntity() { Id = 1, Title = "Admin" },
            new PermissionEntity() { Id = 2, Title = "Inventory management" },
            new PermissionEntity() { Id = 3, Title = "Customers management" },
            new PermissionEntity() { Id = 4, Title = "Orders management" },
            new PermissionEntity() { Id = 5, Title = "Admins management" },
            new PermissionEntity() { Id = 6, Title = "System settings" }
        );
    }
}
