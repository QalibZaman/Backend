using MediatR;
using System.Data;
using NettoRetail.Application.CQRS.Variations.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.VariationTests;

public class CreateVariationsCommandHandlerTests
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateVariationsCommandHandler _handler;

    public CreateVariationsCommandHandlerTests()
    {
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _inventoryService = Substitute.For<IInventoryService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.BeginTransaction().Returns(Substitute.For<IDbTransaction>());

        _handler = new CreateVariationsCommandHandler(
            _inventoryItemRepository,
            _inventoryService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenParentItemIsVariation()
    {
        var request = CreateRequest();
        var parent = CreateParent(isVariation: true, hasVariations: false);

        _inventoryItemRepository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(parent);

        var command = new CreateVariationsCommand(request, 7);

        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUpcAlreadyExists()
    {
        var request = CreateRequest();
        var parent = CreateParent(isVariation: false, hasVariations: false);

        _inventoryItemRepository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(parent);
        _inventoryItemRepository.ExistsAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetProductByUpcSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateVariationsCommand(request, 7);

        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldMarkParentAsHavingVariations_WhenRequestIsValid()
    {
        var request = CreateRequest();
        var parent = CreateParent(isVariation: false, hasVariations: false);
        var builtVariations = new List<InventoryItemEntity>
        {
            CreateParent(isVariation: true, hasVariations: false)
        };

        _inventoryItemRepository.GetAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(parent);
        _inventoryItemRepository.ExistsAsync(Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetProductByUpcSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _inventoryService.BuildVariationInventoryItemEntities(request.Variations, parent)
            .Returns(builtVariations);

        var command = new CreateVariationsCommand(request, 7);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        await _inventoryItemRepository.Received(1)
            .UpdateAsync(Arg.Is<InventoryItemEntity>(x => x.HasVariations), Arg.Any<CancellationToken>());
        await _inventoryItemRepository.Received(1)
            .AddRangeAsync(builtVariations, Arg.Any<CancellationToken>());
    }

    private static CreateVariationsRequest CreateRequest()
    {
        return new CreateVariationsRequest
        {
            InventoryItemId = Guid.NewGuid(),
            Variations =
            [
                new VariationModel
                {
                    ProductName = "Variant_1",
                    ProductUpc = "123456789012",
                    IsActive = true
                }
            ]
        };
    }

    private static InventoryItemEntity CreateParent(bool isVariation, bool hasVariations)
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Parent",
            ProductUpc = "999999999999",
            Price = 100m,
            Description = "Parent description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 5,
            ReservedAmount = 1,
            OnHandAmount = 6,
            ProductLocation = "Warehouse",
            ShopId = 7,
            HasVariations = hasVariations,
            IsVariation = isVariation,
            Exists = true,
            Category = null!,
            BrandId = 1,
            Brand = null!,
            Images = [],
            OrderItems = null!,
            ComparisonItems = null!
        };
    }
}
