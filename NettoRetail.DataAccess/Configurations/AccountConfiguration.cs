using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(30);
        builder.Property(p => p.LastName).HasMaxLength(30);
        builder.Property(p => p.Email).HasMaxLength(256);
        builder.HasIndex(p => p.Email).IsUnique();
        builder.Property(p => p.PasswordHash).HasMaxLength(100);

        builder.Property(p => p.City).HasMaxLength(50);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);
        builder.Property(p => p.ShippingAddress).HasMaxLength(300);
        builder.Property(p => p.ZipCode).HasMaxLength(20);

        builder.Property(p => p.RoleId);

        builder.HasOne(s => s.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(c => c.RoleId)
            .IsRequired(false);

        builder.HasMany(s => s.PaymentMethods)
            .WithOne(r => r.Account)
            .HasForeignKey(c => c.AccountId)
            .IsRequired(false);

        builder.HasOne(s => s.Country)
          .WithMany(r => r.Accounts)
          .HasForeignKey(c => c.CountryId)
          .IsRequired(false);

        builder.HasMany(a => a.PasswordHistories)
          .WithOne(ph => ph.Account)
          .HasForeignKey(ph => ph.AccountId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Status).HasConversion<string>();

        builder.Property(p => p.PasswordUpdatedAt)
            .IsRequired(false);

        builder.HasMany(s => s.Feedbacks)
            .WithOne(f => f.Account)
            .HasForeignKey(f => f.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.CommunicationPreference)
            .WithOne(p => p.User)
            .HasForeignKey<CommunicationPreferenceEntity>(p => p.UserId);

        builder.Property(f => f.DateCreated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
