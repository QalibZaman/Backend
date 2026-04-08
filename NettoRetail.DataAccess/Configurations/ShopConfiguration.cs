using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;

namespace NettoRetail.DataAccess.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<ShopEntity>
{
    public void Configure(EntityTypeBuilder<ShopEntity> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
             .HasMaxLength(90);
        builder.HasIndex(s => s.Name)
             .IsUnique();

        builder.Property(s => s.Email)
            .HasMaxLength(256);
        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);
        builder.HasIndex(s => s.PhoneNumber)
            .IsUnique();

        builder.Property(s => s.Url)
            .HasMaxLength(255);

        builder.Property(s => s.Address)
            .HasMaxLength(90);

        builder.Property(s => s.City)
            .HasMaxLength(50);

        builder.Property(s => s.State)
            .HasMaxLength(50);

        builder.Property(s => s.ZipCode)
            .HasMaxLength(10);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.SubscriptionPlanId)
            .HasDefaultValue(SubscriptionConstants.Free);

        builder.HasOne(s => s.Country)
            .WithMany(c => c.Shops)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(model => model.Roles)
            .WithOne(model => model.Shop)
            .HasForeignKey(model => model.ShopId);

        builder.HasMany(model => model.Categories)
        .WithOne(model => model.Shop)
        .HasForeignKey(model => model.ShopId);

        builder.Navigation(s => s.Country)
            .AutoInclude();
    }
}