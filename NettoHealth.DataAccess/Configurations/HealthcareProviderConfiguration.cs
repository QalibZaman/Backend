using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoHealth.Domain.Entities;

namespace NettoHealth.DataAccess.Configurations;

public class HealthcareProviderConfiguration : IEntityTypeConfiguration<HealthcareProviderEntity>
{
    public void Configure(EntityTypeBuilder<HealthcareProviderEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(30);

        builder.Property(p => p.LastName).HasMaxLength(30);

        builder.Property(p => p.Email).HasMaxLength(100);

        builder.Property(p => p.PasswordHash).HasMaxLength(100);

        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(30);

        builder.Property(p => p.Subscription).HasConversion<string>().HasMaxLength(30);

        builder.Property(p => p.Speciality).HasConversion<string>().HasMaxLength(30);
    }
}