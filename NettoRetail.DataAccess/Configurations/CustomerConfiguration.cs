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
            .IsUnique(); // if customers must have unique emails

        builder.Property(x => x.FirstName)
            .HasMaxLength(30);

        builder.Property(x => x.LastName)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.Customers)
            .HasForeignKey(x => x.RoleId);

        builder.HasOne(x => x.CommunicationPreference)
            .WithOne(x => x.Customer)
            .HasForeignKey<CommunicationPreferenceEntity>(x => x.CustomerId);
    }
}