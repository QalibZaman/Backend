using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations
{
    public class FavouriteListItemConfiguration : IEntityTypeConfiguration<FavouriteListItemEntity>
    {
        public void Configure(EntityTypeBuilder<FavouriteListItemEntity> builder)
        {
            builder.HasKey(model => new { model.AccountId, model.InventoryItemId });

            builder.HasOne(model => model.Account)
                .WithMany(model => model.FavouriteListItems)
                .HasForeignKey(model => model.AccountId);

            builder.HasOne(model => model.InventoryItem)
                .WithMany(model => model.FavouriteListItems)
                .HasForeignKey(model => model.InventoryItemId);
        }
    }
}
