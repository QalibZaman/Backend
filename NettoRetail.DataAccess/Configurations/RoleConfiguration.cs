using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

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
            new RoleEntity { Id = 1, RoleName = RoleEnum.Admin },
            new RoleEntity { Id = 2, RoleName = RoleEnum.SalesManager },
            new RoleEntity { Id = 3, RoleName = RoleEnum.Customer },
            new RoleEntity { Id = 4, RoleName = RoleEnum.InventoryManager },
            new RoleEntity { Id = 5, RoleName = RoleEnum.PosCashier },
            new RoleEntity { Id = 6, RoleName = RoleEnum.ManagementStakeholder },
            new RoleEntity { Id = 7, RoleName = RoleEnum.Shipper },
            new RoleEntity { Id = 8, RoleName = RoleEnum.CustomerSupportSpecialist });
    }
}