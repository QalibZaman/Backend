using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.Application.Services.VariationService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class VariationServiceTests
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IProductDetailRepository _productDetailRepository;
    private readonly VariationService _service;

    public VariationServiceTests()
    {
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _productDetailRepository = Substitute.For<IProductDetailRepository>();
        var imageUploadService = Substitute.For<IImageUploadService>();
        var imageValidatorService = Substitute.For<IImageValidatorService>();

        _inventoryItemRepository
            .UpdateAsync(Arg.Any<InventoryItemEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<InventoryItemEntity>()));

        _service = new VariationService(
            _inventoryItemRepository,
            _productDetailRepository,
            imageUploadService,
            imageValidatorService);
    }

    [Fact]
    public async Task UpdateVariationAsync_ShouldUseParentDescription_WhenDescriptionIsInherited()
    {
        var variation = CreateVariation();
        var parent = CreateParent();

        _inventoryItemRepository
            .GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(variation, parent);

        var model = CreateModel();
        model.ApplyDescriptionFromParent = true;
        model.Description = null;
        model.ApplyDetailsFromParent = true;
        model.ApplyImagesFromParent = true;
        model.ProductUpc = variation.ProductUpc;

        await _service.UpdateVariationAsync(model, 7, CancellationToken.None);

        await _inventoryItemRepository.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(item => item.Description == parent.Description), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateVariationAsync_ShouldReplaceExistingDetails_WhenInheritingParentDetails()
    {
        var variation = CreateVariation();
        variation.ProductDetails.Add(new ProductDetailEntity
        {
            Id = Guid.NewGuid(),
            InventoryItemId = variation.Id,
            Name = "Legacy",
            TypeEnum = ProductDetailTypeEnum.String,
            Unit = null,
            Value = "Old value",
            OrderIndex = 0
        });

        var parent = CreateParent();
        parent.ProductDetails.Add(new ProductDetailEntity
        {
            Id = Guid.NewGuid(),
            InventoryItemId = parent.Id,
            Name = "Material",
            TypeEnum = ProductDetailTypeEnum.String,
            Unit = null,
            Value = "Cotton",
            OrderIndex = 0
        });

        _inventoryItemRepository
            .GetAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(variation, parent);

        var model = CreateModel();
        model.ApplyDescriptionFromParent = true;
        model.Description = null;
        model.ApplyDetailsFromParent = true;
        model.ApplyImagesFromParent = true;
        model.ProductUpc = variation.ProductUpc;

        await _service.UpdateVariationAsync(model, 7, CancellationToken.None);

        await _productDetailRepository.Received(1)
            .DeleteRangeAsync(Arg.Is<IEnumerable<ProductDetailEntity>>(details => details.Single().Name == "Legacy"), Arg.Any<CancellationToken>());

        await _productDetailRepository.Received(1)
            .AddRangeAsync(Arg.Is<IEnumerable<ProductDetailEntity>>(details =>
                details.Single().Name == "Material" &&
                details.Single().InventoryItemId == variation.Id), Arg.Any<CancellationToken>());

        Assert.Single(variation.ProductDetails);
        Assert.Equal("Material", variation.ProductDetails.Single().Name);
    }

    private static UpdateVariationRequest CreateModel()
    {
        return new UpdateVariationRequest
        {
            InventoryItemId = Guid.NewGuid(),
            ProductUpc = "123456789012",
            IsActive = true,
            ApplyDescriptionFromParent = false,
            Description = "Child description",
            ApplyPromotion = false,
            Price = 125m,
            PromotionPrice = null,
            ApplyImagesFromParent = true,
            Status = nameof(InventoryItemStatusEnum.Available),
            AvailableAmount = 6,
            ReservedAmount = 1,
            OnHandAmount = 7,
            ApplyDetailsFromParent = true
        };
    }

    private static InventoryItemEntity CreateVariation()
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Variant",
            ProductUpc = "123456789012",
            Price = 100m,
            Description = "Existing description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 3,
            ReservedAmount = 1,
            OnHandAmount = 4,
            ProductLocation = "Warehouse 1",
            ShopId = 7,
            BrandId = 11,
            Brand = null,
            Category = null!,
            IsVariation = true,
            ParentItemId = Guid.NewGuid()
        };
    }

    private static InventoryItemEntity CreateParent()
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Parent",
            ProductUpc = "999999999999",
            Price = 100m,
            Description = "Parent description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow.AddDays(-2),
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 10,
            ReservedAmount = 2,
            OnHandAmount = 12,
            ProductLocation = "Warehouse 1",
            ShopId = 7,
            BrandId = 11,
            Brand = null,
            Category = null!,
            IsVariation = false
        };
    }
}
