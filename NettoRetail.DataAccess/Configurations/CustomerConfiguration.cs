using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.HasKey(x => x.AccountId);

        builder.HasOne(x => x.Account)
            .WithOne()
            .HasForeignKey<CustomerEntity>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Order)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId);
    }
}