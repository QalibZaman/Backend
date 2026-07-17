using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class BaseUserConfiguration : IEntityTypeConfiguration<BaseUserEntity>
{
    public void Configure(EntityTypeBuilder<BaseUserEntity> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(30);
        builder.Property(x => x.LastName).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(256);
    }
}