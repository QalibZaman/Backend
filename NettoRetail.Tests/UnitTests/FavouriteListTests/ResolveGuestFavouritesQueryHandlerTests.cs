using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.CQRS.FavouriteListItems.Queries;
using NettoRetail.Application.MappingConfiguration;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouritesTests
{
    public class ResolveGuestFavouritesQueryHandlerTests
    {
        private static IMapper CreateMapper()
        {
            var config = new TypeAdapterConfig();
            config.Apply(new MappingConfiguration());

            var services = new ServiceCollection();
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IMapper>();
        }


        private static InventoryItemEntity Item(
            Guid id, int shopId, string name, string categoryName, decimal price, decimal? promo, int available) =>
            new InventoryItemEntity
            {
                Id = id,
                ShopId = shopId,
                ProductName = name,
                ProductUpc = "123456789012",
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
                    Name = categoryName,
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

        [Fact]
        public async Task Handle_ShouldReturnEmpty_AndNotHitRepo_WhenNoIds()
        {
            var repo = Substitute.For<IInventoryItemRepository>();
            var handler = new ResolveGuestFavouritesQueryHandler(repo, CreateMapper());

            var result = await handler.Handle(new ResolveGuestFavouritesQuery(Array.Empty<Guid>(), 9), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.AvailableItemCount);
            Assert.Equal(0m, result.TotalPriceAvailable);

            await repo.DidNotReceive().GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldResolveItems_DiscountPreferred_ExcludeOutOfStock()
        {
            var shopId = 3;
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var itemsDb = new List<InventoryItemEntity>
            {
                Item(id1, shopId, "iPhone 15",  "Smartphones", 999m, 899m, available: 2),
                Item(id2, shopId, "Pixel 8",    "Smartphones", 699m, null,  available: 0)
            };

            var repo = Substitute.For<IInventoryItemRepository>();
            repo.GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(itemsDb);

            var handler = new ResolveGuestFavouritesQueryHandler(repo, CreateMapper());

            var result = await handler.Handle(new ResolveGuestFavouritesQuery(new[] { id1, id2 }, shopId), CancellationToken.None);

            Assert.Equal(2, result.Items.Count());
            Assert.True(result.Items.First(i => i.Id == id2).IsOutOfStock);

            // only in-stock iPhone counts with discount
            Assert.Equal(899m, result.TotalPriceAvailable);
            Assert.Equal(1, result.AvailableItemCount);

            var a = result.Items.First(i => i.Id == id1);
            Assert.False(string.IsNullOrWhiteSpace(a.ThumbnailUrl));
            Assert.Equal("Smartphones", a.CategoryName);
            Assert.Equal(2, a.AvailableAmount);
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationToken_ToRepository()
        {
            var repo = Substitute.For<IInventoryItemRepository>();
            repo.GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(new List<InventoryItemEntity>());

            var handler = new ResolveGuestFavouritesQueryHandler(repo, CreateMapper());
            var cts = new CancellationTokenSource();

            await handler.Handle(new ResolveGuestFavouritesQuery(new[] { Guid.NewGuid() }, 11), cts.Token);

            await repo.Received(1).GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), cts.Token);
        }
    }
}
