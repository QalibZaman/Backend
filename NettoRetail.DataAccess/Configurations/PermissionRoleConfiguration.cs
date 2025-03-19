using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.DataAccess.Configurations
{
    public class PermissionRoleConfiguration : IEntityTypeConfiguration<PermissionRoleEntity>
    {
        public void Configure(EntityTypeBuilder<PermissionRoleEntity> builder)
        {
            builder.ToTable("Permission_Role");

            builder.HasKey(model => new { model.PermissionId, model.RoleId});

            builder.HasOne(model => model.Permission)
                .WithMany(model => model.PermissionRoles)
                .HasForeignKey(model => model.PermissionId);

            builder.HasOne(model => model.Role)
                .WithMany(model => model.PermissionRoles)
                .HasForeignKey(model => model.RoleId);

            builder.Property(model => model.IsAllowed);
        }
    }
}
