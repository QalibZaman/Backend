using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.HasMany(x => x.Order)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId);

        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
    }
}