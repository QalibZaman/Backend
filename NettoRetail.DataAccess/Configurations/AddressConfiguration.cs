using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
    {
        public void Configure(EntityTypeBuilder<AddressEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Street)
                .HasMaxLength(100);

            builder.Property(x => x.HouseNumber)
                .HasMaxLength(20);

            builder.Property(x => x.Apartment)
                .HasMaxLength(20);

            builder.Property(x => x.City)
                .HasMaxLength(50);

            builder.Property(x => x.State)
                .HasMaxLength(50);

            builder.Property(x => x.ZipCode)
                .HasMaxLength(20);

            builder.HasOne(x => x.Country)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                .WithOne(x => x.Address)
                .HasForeignKey<AddressEntity>(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Staff)
                .WithOne(x => x.Address)
                .HasForeignKey<AddressEntity>(x => x.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.CustomerId)
                .IsUnique();

            builder.HasIndex(x => x.StaffId)
                .IsUnique();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Address_OnlyOneOwner",
                    "(\"CustomerId\" IS NOT NULL AND \"StaffId\" IS NULL) OR (\"CustomerId\" IS NULL AND \"StaffId\" IS NOT NULL)");
            });
        }
    }
}
