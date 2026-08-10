using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customers");

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany(r => r.Customers)
            .HasForeignKey(x => x.RoleId);

        builder.HasOne(x => x.CommunicationPreference)
            .WithOne(x => x.Customer)
            .HasForeignKey<CommunicationPreferenceEntity>(x => x.CustomerId);
    }
}