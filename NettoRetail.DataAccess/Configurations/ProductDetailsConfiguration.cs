using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

public class ProductDetailsConfiguration : IEntityTypeConfiguration<ProductDetailsEntity>
{
    public void Configure(EntityTypeBuilder<ProductDetailsEntity> builder)
    {
        builder.ToContainer("ProductDetailsContainer")
            .HasPartitionKey(p => p.InventoryItemId);
    }
}
