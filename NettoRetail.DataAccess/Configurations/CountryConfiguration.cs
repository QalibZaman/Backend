using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<CountryEntity>
{
    public void Configure(EntityTypeBuilder<CountryEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(50);
        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.Property(c => c.PhoneCode)
            .HasMaxLength(4);
        builder.HasIndex(c => c.PhoneCode)
            .IsUnique();

        builder.Property(c => c.PhoneMask)
            .HasMaxLength(30);

        builder.Property(c => c.ZipCodeMask)
            .HasMaxLength(30);

        builder.HasData(
            new CountryEntity { Id = 1, Name = "Austria", PhoneCode = "+43", PhoneMask = "^\\d{3} \\d{7}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 2, Name = "Belgium", PhoneCode = "+32", PhoneMask = "^\\d \\d{3} \\d{2} \\d{2}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 3, Name = "Bulgaria", PhoneCode = "+359", PhoneMask = "^\\d{2} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 4, Name = "Croatia", PhoneCode = "+385", PhoneMask = "^\\d{2} \\d{3} \\d{4}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 5, Name = "Cyprus", PhoneCode = "+357", PhoneMask = "^2\\d \\d{6}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 6, Name = "Czech Republic", PhoneCode = "+420", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 7, Name = "Denmark", PhoneCode = "+45", PhoneMask = "^\\d{2} \\d{2} \\d{2} \\d{2}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 8, Name = "Estonia", PhoneCode = "+372", PhoneMask = "^\\d{4} \\d{4}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 9, Name = "Finland", PhoneCode = "+358", PhoneMask = "^\\d \\d{3} \\d{4}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 10, Name = "France", PhoneCode = "+33", PhoneMask = "^\\d \\d{2} \\d{2} \\d{2} \\d{2}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 11, Name = "Germany", PhoneCode = "+49", PhoneMask = "^\\d{3} \\d{3} \\d{4}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 12, Name = "Greece", PhoneCode = "+30", PhoneMask = "^\\d{3} \\d{3} \\d{4}$", ZipCodeMask = "^\\d{3} \\d{2}$" },
            new CountryEntity { Id = 13, Name = "Hungary", PhoneCode = "+36", PhoneMask = "^\\d{2} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 14, Name = "Ireland", PhoneCode = "+353", PhoneMask = "^\\d \\d{3} \\d{4}$", ZipCodeMask = "^[A-Z]\\d{2}$" },
            new CountryEntity { Id = 15, Name = "Italy", PhoneCode = "+39", PhoneMask = "^\\d{2} \\d{4} \\d{4}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 16, Name = "Latvia", PhoneCode = "+371", PhoneMask = "^\\d{2} \\d{3} \\d{3}$", ZipCodeMask = "^LV-\\d{4}$" },
            new CountryEntity { Id = 17, Name = "Lithuania", PhoneCode = "+370", PhoneMask = "^\\d{3} \\d{5}$", ZipCodeMask = "^LT-\\d{5}$" },
            new CountryEntity { Id = 18, Name = "Luxembourg", PhoneCode = "+352", PhoneMask = "^\\d{3} \\d{3}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 19, Name = "Malta", PhoneCode = "+356", PhoneMask = "^\\d{4} \\d{4}$", ZipCodeMask = "^[A-Z]{3} \\d{4}$" },
            new CountryEntity { Id = 20, Name = "Netherlands", PhoneCode = "+31", PhoneMask = "^\\d \\d{4} \\d{4}$", ZipCodeMask = "^\\d{4} [A-Z]{2}$" },
            new CountryEntity { Id = 21, Name = "Poland", PhoneCode = "+48", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{2}-\\d{3}$" },
            new CountryEntity { Id = 22, Name = "Portugal", PhoneCode = "+351", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{4}-\\d{3}$" },
            new CountryEntity { Id = 23, Name = "Romania", PhoneCode = "+40", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{6}$" },
            new CountryEntity { Id = 24, Name = "Slovakia", PhoneCode = "+421", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{3} \\d{2}$" },
            new CountryEntity { Id = 25, Name = "Slovenia", PhoneCode = "+386", PhoneMask = "^\\d \\d{4} \\d{4}$", ZipCodeMask = "^\\d{4}$" },
            new CountryEntity { Id = 26, Name = "Spain", PhoneCode = "+34", PhoneMask = "^\\d{3} \\d{3} \\d{3}$", ZipCodeMask = "^\\d{5}$" },
            new CountryEntity { Id = 27, Name = "Sweden", PhoneCode = "+46", PhoneMask = "^\\d{3} \\d{3} \\d{4}$", ZipCodeMask = "^\\d{3} \\d{2}$" });
    }
}
