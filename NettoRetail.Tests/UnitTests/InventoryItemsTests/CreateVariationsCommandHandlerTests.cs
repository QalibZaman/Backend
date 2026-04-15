using System.Data;
using MediatR;
using NettoRetail.Application.CQRS.Variations.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItemsTests;

public class CreateVariationsCommandHandlerTests
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _transaction;
    private readonly CreateVariationsCommandHandler _handler;

    public CreateVariationsCommandHandlerTests()
    {
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _inventoryService = Substitute.For<IInventoryService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new CreateVariationsCommandHandler(
            _inventoryItemRepository,
            _inventoryService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateOnlyNewUniqueVariations()
    {
        var shopId = 11;
        var parent = BuildItem("Parent", isVariation: false);

        var existingVariation = BuildItem("Black 64 GB", isVariation: true, parentItemId: parent.Id);

        _inventoryItemRepository.GetAsync(
                Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(parent);

        _inventoryItemRepository.GetAllBySpecAsync(
                Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.Variations.GetVariationsByParentIdSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity> { existingVariation });

        ICollection<VariationModel>? capturedModels = null;
        var generated = new List<InventoryItemEntity>
        {
            BuildItem("White 64 GB", isVariation: true, parentItemId: parent.Id)
        };

        _inventoryService.BuildVariationInventoryItemEntities(
                Arg.Do<ICollection<VariationModel>>(models => capturedModels = models),
                parent)
            .Returns(generated);

        var request = new CreateVariationsRequest
        {
            InventoryItemId = parent.Id,
            Variations =
            [
                new VariationModel { ProductName = " Black 64 GB ", ProductUpc = string.Empty, IsActive = true },
                new VariationModel { ProductName = "White 64 GB", ProductUpc = string.Empty, IsActive = true },
                new VariationModel { ProductName = "White 64 GB", ProductUpc = string.Empty, IsActive = false }
            ]
        };

        var command = new CreateVariationsCommand(request, shopId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        Assert.NotNull(capturedModels);
        Assert.Single(capturedModels!);
        Assert.Equal("White 64 GB", capturedModels!.Single().ProductName);

        await _inventoryItemRepository.Received(1).UpdateAsync(
            Arg.Is<InventoryItemEntity>(x => x.Id == parent.Id && x.HasVariations),
            Arg.Any<CancellationToken>());

        await _inventoryItemRepository.Received(1).AddRangeAsync(generated, Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenParentItemIsVariation()
    {
        var parentVariation = BuildItem("Child", isVariation: true);

        _inventoryItemRepository.GetAsync(
                Arg.Any<NettoRetail.DataAccess.Specifications.Inventory.GetInventoryItemByIdAndShopIdSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(parentVariation);

        var command = new CreateVariationsCommand(
            new CreateVariationsRequest
            {
                InventoryItemId = parentVariation.Id,
                Variations = [new VariationModel { ProductName = "Size L", ProductUpc = string.Empty, IsActive = true }]
            },
            ShopId: 7);

        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _unitOfWork.DidNotReceive().BeginTransaction();
        await _inventoryItemRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<InventoryItemEntity>>(), Arg.Any<CancellationToken>());
    }

    private static InventoryItemEntity BuildItem(string name, bool isVariation, Guid? parentItemId = null)
    {
        return new InventoryItemEntity
        {
            Id = Guid.NewGuid(),
            ProductName = name,
            ProductUpc = "123456789012",
            Price = 10m,
            Description = "desc",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = 0,
            ReservedAmount = 0,
            OnHandAmount = 0,
            ProductLocation = "A1",
            ShopId = 11,
            HasVariations = false,
            IsVariation = isVariation,
            Exists = true,
            ParentItemId = parentItemId,
            Category = null!,
            BrandId = null,
            Brand = null,
            Images = [],
            OrderItems = [],
            ComparisonItems = []
        };
    }
}
