using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistoryEntity>
{
    public void Configure(EntityTypeBuilder<PasswordHistoryEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Account)
               .WithMany(a => a.PasswordHistories)
               .HasForeignKey(p => p.AccountId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.HashedPassword).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
    }
}
