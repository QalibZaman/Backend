using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
    {
        public void Configure(EntityTypeBuilder<CartEntity> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Customer)
                .WithOne(cu => cu.Cart)
                .HasForeignKey<CartEntity>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
