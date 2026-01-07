using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class InvitationTokenConfiguration : IEntityTypeConfiguration<InvitationTokenEntity>
{
    public void Configure(EntityTypeBuilder<InvitationTokenEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.UsedAt)
            .IsRequired(false);

        builder.Property(t => t.InvalidatedAt)
            .IsRequired(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne(t => t.AccountEntity)
            .WithMany(a => a.InvitationTokens)
            .HasForeignKey(t => t.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
