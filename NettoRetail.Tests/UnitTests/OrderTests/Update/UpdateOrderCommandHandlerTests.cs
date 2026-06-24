using FluentAssertions;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using System.Data;

namespace NettoRetail.Tests.UnitTests.OrderTests.Update;

public class UpdateOrderCommandHandlerTests
{
    private readonly UpdateOrderCommandHandler _handler;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _transaction;

    public UpdateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<IDbTransaction>();


        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new UpdateOrderCommandHandler(_orderRepository, _inventoryItemRepository, _unitOfWork);
    }

    [Fact]
    public async Task Should_UpdateOrder_ValidRequest()
    {
        //Arrange
        var command = SetupValidUpdateOrder();

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _orderRepository.Received(1).UpdateAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>());

        await _inventoryItemRepository.Received(1)
            .UpdateManyAsync(Arg.Any<List<InventoryItemEntity>>(), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Should_ThrowNotFoundException_WhenOrderNotFound()
    {
        //Arrange
        var command = SetupValidUpdateOrder();
        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns((OrderEntity)null!);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Order.OrderNotFoundMessage);

        _unitOfWork.DidNotReceive().BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Should_ThrowBadRequestException_WhenOrderIsInProgress()
    {
        //Arrange
        var command = SetupValidUpdateOrder();
        var inProgressOrder = CreateDefaultOrder(command.Model.CustomerId);
        inProgressOrder.OrderStatus = OrderStatusEnum.InProgress;

        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(inProgressOrder);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(HandlerConstants.Order.OrderCannotBeEditedMessage);

        _unitOfWork.DidNotReceive().BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Should_ThrowNotFoundException_WhenInventoryItemsNotFound()
    {
        //Arrange
        var command = SetupValidUpdateOrder();
        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns((List<InventoryItemEntity>)null!);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Inventory.InventoryItemsNotFoundMessage);

        _unitOfWork.DidNotReceive().BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Should_ThrowBadRequestException_WhenItemStockIsInsufficient()
    {
        //Arrange
        var command = SetupValidUpdateOrder();
        command.Model.ItemsToDelete = null;
        var upsertId = command.Model.ItemsToUpsert.Keys.First();
        var stockItem = CreateDefaultInventoryItem(upsertId, command.ShopId);
        stockItem.AvailableAmount = 0;
        command.Model.ItemsToUpsert[upsertId] = 5;

        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(CreateDefaultOrder(command.Model.CustomerId));

        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity> { stockItem });

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(string.Format(HandlerConstants.Order.InsufficientStockItem, upsertId));

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Should_ThrowBadRequestException_WhenReservedAmountIsLessThanZero()
    {
        //Arrange
        var command = SetupValidUpdateOrder();
        var deleteId = command.Model.ItemsToDelete!.First();
        var shopId = command.ShopId;

        var stockItem = CreateDefaultInventoryItem(deleteId, shopId);
        stockItem.ReservedAmount = 1;

        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity>() { stockItem });

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(string.Format(HandlerConstants.Inventory.ReservedAmountNotEnoughMessage, deleteId));

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Should_CorrectlyUpdateInventory_WhenQuantityIsIncreased()
    {
        //Arrange
        var itemId = Guid.NewGuid();
        var command = new UpdateOrderCommand(
            new UpdateOrderModel
            {
                DeliveryType = OrderDeliveryTypeEnum.Courier,
                CustomerId = Guid.NewGuid(),
                ItemsToUpsert = new Dictionary<Guid, int> { { itemId, 10 } },
                ItemsToDelete = new List<Guid>()
            }, 10, Guid.NewGuid(), 100);

        var order = CreateDefaultOrder(command.Model.CustomerId);
        order.OrderItems.Add(new OrderItemEntity
        { Id = Guid.NewGuid(), InventoryItemId = itemId, Quantity = 4 });
        var stockItem = CreateDefaultInventoryItem(itemId, command.ShopId);
        stockItem.AvailableAmount = 100;
        stockItem.ReservedAmount = 10;

        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(order);
        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity> { stockItem });

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        stockItem.AvailableAmount.Should().Be(94);
        stockItem.ReservedAmount.Should().Be(16);

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Should_RemoveItem_WhenUpsertQuantityIsSetToZero()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var command = new UpdateOrderCommand(
            new UpdateOrderModel
            {
                DeliveryType = OrderDeliveryTypeEnum.Courier, CustomerId = Guid.NewGuid(),
                ItemsToUpsert = new Dictionary<Guid, int> { { itemId, 0 } }
            },
            10, Guid.NewGuid(), 100);

        var order = CreateDefaultOrder(Guid.NewGuid());
        order.OrderItems.Add(new OrderItemEntity { Id = Guid.NewGuid(), InventoryItemId = itemId, Quantity = 5 });

        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>()).Returns(order);

        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(new List<InventoryItemEntity> { CreateDefaultInventoryItem(itemId, 10) });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.OrderItems.Should().NotContain(x => x.InventoryItemId == itemId);

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Should_UpdateDeliveryType_WhenValidRequest()
    {
        var delivery = new DeliveryEntity
        {
            Id = Guid.NewGuid(),
            DeliveryType = OrderDeliveryTypeEnum.Courier,
            DeliveryFee = 12,
            PaymentTotal = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            
        };
        // Arrange
        var command = SetupValidUpdateOrder();
        var newDeliveryType = OrderDeliveryTypeEnum.Courier;
        command.Model.DeliveryType = newDeliveryType;
        command.Model.ItemsToDelete = null;
        var order = CreateDefaultOrder(command.Model.CustomerId);
        delivery.DeliveryType = OrderDeliveryTypeEnum.SelfDelivery;

        _orderRepository.GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        delivery.DeliveryType.Should().Be(newDeliveryType);
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    private UpdateOrderCommand SetupValidUpdateOrder()
    {
        var itemId = Guid.NewGuid();
        var deleteId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var shopId = 10;
        var userId = Guid.NewGuid();
        var orderId = 100;
        var command = new UpdateOrderCommand(
            new UpdateOrderModel
            {
                DeliveryType = OrderDeliveryTypeEnum.SelfDelivery,
                ItemsToDelete = new List<Guid> { deleteId },
                ItemsToUpsert = new Dictionary<Guid, int> { { itemId, 5 } },
                CustomerId = customerId
            },
            ShopId: shopId,
            UserId: userId,
            OrderId: orderId
        );
        var order = CreateDefaultOrder(customerId, deleteId);
        var stockItems = new List<InventoryItemEntity>
        {
            CreateDefaultInventoryItem(itemId, shopId),
            CreateDefaultInventoryItem(deleteId, shopId),
        };

        _orderRepository
            .GetAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
            .Returns(order);

        _inventoryItemRepository
            .GetManyBySpecAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
            .Returns(stockItems);


        return command;
    }

    private OrderEntity CreateDefaultOrder(Guid customerId, Guid? existingItemId = null)
    {
        var delivery = new DeliveryEntity
        {
            Id = Guid.NewGuid(),
            DeliveryType = OrderDeliveryTypeEnum.SelfDelivery,
            DeliveryFee = 12,
            PaymentTotal = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            
        };
        var order = new OrderEntity
        {
            CustomerId = customerId,
            OrderStatus = OrderStatusEnum.New,
            OrderItems = new List<OrderItemEntity>(),
            Delivery = delivery,
            CreationDate = DateTime.UtcNow,
            Creator = "test",
        };

        if (existingItemId.HasValue)
        {
            order.OrderItems.Add(new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                InventoryItemId = existingItemId.Value,
                Quantity = 2,
                OrderId = 100
            });
        }

        return order;
    }

    private InventoryItemEntity CreateDefaultInventoryItem(Guid id, int shopId)
    {
        return new InventoryItemEntity
        {
            Id = id,
            AvailableAmount = 100,
            IncomingAmount = 3,
            ReservedAmount = 10,
            OnHandAmount = 110,
            ShopId = shopId,
            Description = "description",
            LastUpdatedDate = DateTime.UtcNow,
            Brand = new BrandEntity { Id = 10, Name = "brand", ShopId = shopId },
            BrandId = 10,
            Category = new CategoryEntity
                { Id = 10, Name = "category", ShopId = shopId, URL = "https://test.com", Visibility = true },
            CategoryId = 10,
            Price = 100,
            IsActive = true,
            ProductLocation = "location",
            ProductName = "name",
            ProductUpc = "123456789101",
            Status = InventoryItemStatusEnum.Available
        };
    }
}