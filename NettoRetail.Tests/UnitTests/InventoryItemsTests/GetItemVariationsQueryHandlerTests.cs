using NettoRetail.Application.CQRS.Variations.Queries;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItemsTests;

public class GetItemVariationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMapParentIdAndFilterByShopInSpecification()
    {
        // Arrage
        var repository = Substitute.For<IInventoryItemRepository>();
        var mapper = MapperFactory.Create();
        var handler = new GetItemVariationsQueryHandler(repository, mapper);

        var parentId = Guid.NewGuid();
        var shopId = 77;

        repository.GetAllBySpecAsync(
                Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.Variations.GetVariationsByParentIdAndShopIdSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity>
            {
                BuildVariation(parentId, shopId, "Black")
            });

        // Act
        var response = await handler.Handle(new GetItemVariationsQuery(parentId, shopId), CancellationToken.None);

        // Assert
        var model = Assert.Single(response);
        Assert.Equal(parentId, model.ParentId);
        Assert.Equal("Black", model.ProductName);

        await repository.Received(1).GetAllBySpecAsync(
            Arg.Is<NettoRetail.DataAccess.Specifications.Inventory.Variations.GetVariationsByParentIdAndShopIdSpecification>(spec =>
                MatchesShopAndParent(spec, parentId, shopId)),
            Arg.Any<CancellationToken>());
    }

    private static bool MatchesShopAndParent(
        NettoRetail.DataAccess.Specifications.Inventory.Variations.GetVariationsByParentIdAndShopIdSpecification spec,
        Guid parentId,
        int shopId)
    {
        var predicate = spec.Criteria.Compile();

        var validItem = BuildVariation(parentId, shopId, "A");
        var wrongShopItem = BuildVariation(parentId, shopId + 1, "B");
        var wrongParentItem = BuildVariation(Guid.NewGuid(), shopId, "C");

        return predicate(validItem)
            && !predicate(wrongShopItem)
            && !predicate(wrongParentItem);
    }

    private static InventoryItemEntity BuildVariation(Guid parentId, int shopId, string name)
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = name,
            ProductUpc = "123456789012",
            Price = 12m,
            Description = "desc",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 0,
            ReservedAmount = 0,
            OnHandAmount = 0,
            ProductLocation = "A1",
            ShopId = shopId,
            HasVariations = false,
            IsVariation = true,
            Exists = true,
            ParentItemId = parentId,
            Category = null!,
            BrandId = null,
            Brand = null,
            Images = [],
            OrderItems = [],
            ComparisonItems = []
        };
    }
}
