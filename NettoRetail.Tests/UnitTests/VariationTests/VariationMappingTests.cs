using MapsterMapper;
using NettoRetail.Application.Models.Response.InventoryItem.Variations;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Tests.Helpers;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class VariationMappingTests
{
    private readonly IMapper _mapper = MapperFactory.Create();

    [Fact]
    public void Map_ShouldPopulateParentId_FromParentItemId()
    {
        var parentId = Guid.NewGuid();
        var entity = new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ParentItemId = parentId,
            ProductName = "Variant",
            ProductUpc = "123456789012",
            Price = 100m,
            Description = "Description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 1,
            ReservedAmount = 0,
            OnHandAmount = 1,
            ProductLocation = "Shelf A",
            ShopId = 7,
            HasVariations = false,
            IsVariation = true,
            Exists = true,
            Category = null!,
            BrandId = 1,
            Brand = null!,
            Images = [],
            OrderItems = null!,
            ComparisonItems = null!
        };

        var result = _mapper.Map<VariationResponse>(entity);

        Assert.Equal(parentId, result.ParentId);
    }
}
