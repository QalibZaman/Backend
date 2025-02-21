using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.DataAccess.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RoleName).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(r => r.StaffEntities)
            .WithOne(s => s.Role)
            .HasForeignKey(c => c.RoleId);

        builder.HasData(
            new RoleEntity { Id = RoleIdConstants.AdminId, RoleName = RoleEnum.Admin },
            new RoleEntity { Id = RoleIdConstants.SalesManagerId, RoleName = RoleEnum.SalesManager },
            new RoleEntity { Id = RoleIdConstants.CustomerId, RoleName = RoleEnum.Customer },
            new RoleEntity { Id = RoleIdConstants.InventoryManagerId, RoleName = RoleEnum.InventoryManager },
            new RoleEntity { Id = RoleIdConstants.PosCashierId, RoleName = RoleEnum.PosCashier },
            new RoleEntity { Id = RoleIdConstants.ManagementStakeholderId, RoleName = RoleEnum.ManagementStakeholder },
            new RoleEntity { Id = RoleIdConstants.ShipperId, RoleName = RoleEnum.Shipper },
            new RoleEntity { Id = RoleIdConstants.CustomerSupportSpecialistId, RoleName = RoleEnum.CustomerSupportSpecialist });
    }
}