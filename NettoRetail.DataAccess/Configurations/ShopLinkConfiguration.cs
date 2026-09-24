using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class ShopLinkConfiguration : IEntityTypeConfiguration<ShopLinkEntity>
    {
        public void Configure(EntityTypeBuilder<ShopLinkEntity> builder)
        {
            builder.ToTable("ShopLinks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url)
                .IsRequired();

            builder.Property(x => x.SocialNetwork)
                .IsRequired();

            builder.HasOne(x => x.Shop)
                .WithMany(x => x.ShopLinks)
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.ShopId,
                x.SocialNetwork
            })
            .IsUnique();
        }
    }
}
