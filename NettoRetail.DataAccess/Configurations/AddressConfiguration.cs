using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Country)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.City).HasMaxLength(90).IsRequired();
        builder.Property(x => x.ZipCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Street).HasMaxLength(90).IsRequired();
        builder.Property(x => x.BuildingNumber).IsRequired();
    }
}
