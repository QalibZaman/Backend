using AutoFixture;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.ShopTests
{
    public class ShopCatalogFilterTests
    {
        private readonly IInventoryItemRepository _itemsRepo;
        private readonly Fixture _fixture;
        private readonly int _shopId = 1;

        public ShopCatalogFilterTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<DateOnly>(c => c.FromFactory(() => DateOnly.FromDateTime(DateTime.Now)));

            _itemsRepo = Substitute.For<IInventoryItemRepository>();
        }

        [Fact]
        public async Task ShopCatalogFilter_ShouldReturnPaginatedResponse_WhenRequestIsValid()
        {
            // Arrange
            var category = new CategoryEntity { Id = 1, Name = "Cat1", ShopId = _shopId, URL = "cat", Visibility = true };

            var itemsList = new List<InventoryItemEntity>
            {
                _fixture.Build<InventoryItemEntity>().With(x => x.ShopId, _shopId).With(x => x.Category, category).OmitAutoProperties().Create(),
                _fixture.Build<InventoryItemEntity>().With(x => x.ShopId, _shopId).With(x => x.Category, category).OmitAutoProperties().Create(),
                _fixture.Build<InventoryItemEntity>().With(x => x.ShopId, _shopId).With(x => x.Category, category).OmitAutoProperties().Create()
            };

            var handler = new GetShopFilteredCatalogHandler(_itemsRepo);

            var request = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    IsPromoted = true,
                    ProductName = "Sample"
                },
                ShopId: _shopId,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            _itemsRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                    .Returns((itemsList, itemsList.Count));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Equal(itemsList.Count, result.Items.Count);
            Assert.Equal(itemsList.Count, result.TotalAmount);
        }

        [Fact]
        public async Task ShopCatalogFilter_ShouldReturnEmptyList_WhenNoItems()
        {
            // Arrange
            var itemsList = new List<InventoryItemEntity>();

            var handler = new GetShopFilteredCatalogHandler(_itemsRepo);

            var request = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    IsPromoted = true,
                    ProductName = "Sample"
                },
                ShopId: _shopId,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            _itemsRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                    .Returns((itemsList, itemsList.Count));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
        }

        [Fact]
        public async Task ShopCatalogFilter_ShouldNotFail_WhenFiltersAreEmpty()
        {
            // Arrange
            var itemsList = new List<InventoryItemEntity>();

            var handler = new GetShopFilteredCatalogHandler(_itemsRepo);

            var request = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel(),
                ShopId: _shopId,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            _itemsRepo.GetAllPaginatedAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                    .Returns((itemsList, itemsList.Count));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
        }
    }
}
