using Mapster;
using NettoRetail.Application.CQRS.FavouriteListItems.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouritesTests
{
    public class GetFavouriteListQueryHandlerTests
    {
        public GetFavouriteListQueryHandlerTests()
        {
            // Ensure Mapster config is registered
            TypeAdapterConfig.GlobalSettings.Scan(typeof(InventoryItemCardResponse).Assembly);
        }


        private static InventoryItemEntity Item(
            Guid id, int shopId, string name, string categoryName, decimal price, decimal? promo, int available) =>
            new InventoryItemEntity
            {
                Id = id,
                ShopId = shopId,
                ProductName = name,
                ProductUpc = 123456789012,
                Price = price,
                Description = "smartphone",
                CategoryId = 1,
                PromotionPrice = promo,
                IsPromoted = promo.HasValue,
                LastUpdatedDate = DateTime.UtcNow,
                IsActive = true,
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = available,
                ReservedAmount = 0,
                OnHandAmount = Math.Max(available, 0),
                ProductLocation = "A1",
                Category = new CategoryEntity
                {
                    Id = 1,
                    Name = categoryName, // "Smartphones"
                    URL = "smartphones",
                    Visibility = true,
                    Level = 1,
                    ParentId = null,
                    ShopId = shopId
                },
                BrandId = null,
                Brand = null,
                Images = new List<InventoryItemImageEntity>
                {
                    new InventoryItemImageEntity { Id = Guid.NewGuid(), BlobUrl = "https://cdn.test/thumb.jpg", Order = 0 }
                }
            };

        private static FavouriteListItemEntity Fav(Guid accountId, int shopId, InventoryItemEntity item) =>
            new FavouriteListItemEntity
            {
                AccountId = accountId,
                InventoryItemId = item.Id,
                InventoryItem = item
            };

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoFavourites()
        {
            var repo = Substitute.For<IFavouriteListItemRepository>();
            repo.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(new List<FavouriteListItemEntity>());

            var handler = new GetFavouriteListQueryHandler(repo);
            var result = await handler.Handle(new GetFavouriteListQuery(Guid.NewGuid(), 1), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.AvailableItemCount);
            Assert.Equal(0m, result.TotalPriceAvailable);
            await repo.Received(1).GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldComputeTotals_DiscountPreferred_ExcludeOutOfStock_AndExposeAvailableAmount()
        {
            var userId = Guid.NewGuid();
            var shopId = 5;

            var iphone15 = Item(Guid.NewGuid(), shopId, "iPhone 15", "Smartphones", 999m, 899m, available: 3);
            var galaxyS24 = Item(Guid.NewGuid(), shopId, "Galaxy S24", "Smartphones", 799m, null, available: 1);
            var pixel8 = Item(Guid.NewGuid(), shopId, "Pixel 8", "Smartphones", 699m, 649m, available: 0);

            var data = new List<FavouriteListItemEntity>
            {
                Fav(userId, shopId, iphone15),
                Fav(userId, shopId, galaxyS24),
                Fav(userId, shopId, pixel8)
            };

            var repo = Substitute.For<IFavouriteListItemRepository>();
            repo.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(data);

            var handler = new GetFavouriteListQueryHandler(repo);
            var result = await handler.Handle(new GetFavouriteListQuery(userId, shopId), CancellationToken.None);

            Assert.Equal(3, result.Items.Count());
            Assert.True(result.Items.First(i => i.Id == pixel8.Id).IsOutOfStock);

            // discounted iPhone 15 + full-price Galaxy S24; Pixel 8 is OOS
            Assert.Equal(899m + 799m, result.TotalPriceAvailable);
            Assert.Equal(2, result.AvailableItemCount);

            Assert.Equal(3, result.Items.First(i => i.Id == iphone15.Id).AvailableAmount);
            Assert.Equal(1, result.Items.First(i => i.Id == galaxyS24.Id).AvailableAmount);
            Assert.Equal(0, result.Items.First(i => i.Id == pixel8.Id).AvailableAmount);

            var first = result.Items.First();
            Assert.False(string.IsNullOrWhiteSpace(first.ProductName));
            Assert.Equal("Smartphones", first.CategoryName);
            Assert.False(string.IsNullOrWhiteSpace(first.ThumbnailUrl));
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationToken_ToRepository()
        {
            var repo = Substitute.For<IFavouriteListItemRepository>();
            repo.GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(new List<FavouriteListItemEntity>());

            var handler = new GetFavouriteListQueryHandler(repo);
            var cts = new CancellationTokenSource();

            await handler.Handle(new GetFavouriteListQuery(Guid.NewGuid(), 7), cts.Token);

            await repo.Received(1).GetManyBySpecAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), cts.Token);
        }
    }
}
