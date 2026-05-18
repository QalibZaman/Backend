using System.Data;
using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests.Create;

public class CreateOrderCommandHandlerTests
{
    private readonly CreateOrderCommandHandler _handler;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDbTransaction _transaction;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_transaction);
        _orderRepository.AddAsync(Arg.Any<OrderEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<OrderEntity>());

        _mapper.Map<OrderEntity>(Arg.Any<CreateOrderModel>())
            .Returns(callInfo =>
            {
                var model = callInfo.Arg<CreateOrderModel>();
                return new OrderEntity
                {
                    Id = 0,
                    CustomerId = model.CustomerId,
                    Delivery = new DeliveryEntity { DeliveryType = model.DeliveryType },
                    Creator = string.Empty,
                    OrderStatus = OrderStatusEnum.New,
                    CreationDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItemEntity>()
                };
            });

        _mapper.Map<OrderResponse>(Arg.Any<OrderEntity>())
            .Returns(callInfo =>
            {
                var source = callInfo.Arg<OrderEntity>();
                return new OrderResponse
                {
                    Id = source.Id,
                    CustomerFirstName = string.Empty,
                    CustomerLastName = string.Empty,
                    CustomerEmail = string.Empty,
                    CreationDate = source.CreationDate,
                    DeliveryType = source.Delivery?.DeliveryType.ToString(),
                    Creator = source.Creator,
                    OrderStatus = source.OrderStatus.ToString(),
                    CustomerId = source.CustomerId,
                    OrderItems = []
                };
            });

        _handler = new CreateOrderCommandHandler(
            _orderRepository,
            _customerRepository,
            _staffRepository,
            _inventoryItemRepository,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Should_SetCreatorFromAuthenticatedStaff_WhenOrderIsCreated()
    {
        // Arrange
        var command = CreateCommand(shopId: 10, amount: 2);
        var staff = CreateStaff(command.UserId, "John", "Creator");

        ConfigureHappyPath(command, availableAmount: 5, staff: staff);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _orderRepository.Received(1).AddAsync(
            Arg.Is<OrderEntity>(o => o.Creator == "John Creator"),
            Arg.Any<CancellationToken>());
        result.Creator.Should().Be("John Creator");
    }

    [Fact]
    public async Task Should_ThrowConflictException_WithAvailableAmount_WhenStockIsInsufficient()
    {
        // Arrange
        var command = CreateCommand(shopId: 10, amount: 4);
        ConfigureHappyPath(command, availableAmount: 3);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Item is not available in this quantity. (Available 3)");
    }

    [Fact]
    public async Task Should_UseShopScopedSpecifications_ForInventoryAndCustomer()
    {
        // Arrange
        var command = CreateCommand(shopId: 55, amount: 1);
        ConfigureHappyPath(command, availableAmount: 10);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _inventoryItemRepository.Received(1).GetRangeByIdsAsync(
            Arg.Is<Specification<InventoryItemEntity>>(s => s is GetInventoryItemsByIdsAndShopSpecification),
            Arg.Any<CancellationToken>());
        _customerRepository.Received(1).GetWithRelatedDataAsync(
            Arg.Is<Specification<CustomerEntity>>(s => s is GetCustomerByIdAndShopIdSpecification),
            Arg.Any<CancellationToken>());
        _staffRepository.Received(1).GetAsync(
            Arg.Is<Specification<StaffEntity>>(s => s is GetStaffByIdSpecification),
            Arg.Any<CancellationToken>());
    }

    private void ConfigureHappyPath(
        CreateOrderCommand command,
        int availableAmount,
        StaffEntity? staff = null)
    {
        var productId = command.Model.Products.Single().InventoryItemId;
        var inventoryItem = CreateInventoryItem(productId, command.ShopId, availableAmount);

        _inventoryItemRepository.GetRangeByIdsAsync(
            Arg.Any<Specification<InventoryItemEntity>>(),
            Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable([inventoryItem]));

        _staffRepository.GetAsync(Arg.Any<Specification<StaffEntity>>(), Arg.Any<CancellationToken>())
            .Returns(staff ?? CreateStaff(command.UserId, "Test", "Staff"));

        _customerRepository.GetWithRelatedDataAsync(
                Arg.Any<Specification<CustomerEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateCustomer(command.Model.CustomerId, command.ShopId));
    }

    private static CreateOrderCommand CreateCommand(int shopId, int amount)
    {
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var model = new CreateOrderModel
        {
            CustomerId = customerId,
            DeliveryType = OrderDeliveryTypeEnum.Courier,
            Products = [new OrderProductDto { InventoryItemId = productId, Amount = amount }]
        };

        return new CreateOrderCommand(model, shopId, userId);
    }

    private static CustomerEntity CreateCustomer(Guid id, int shopId)
    {
        return new CustomerEntity
        {
            Id = id,
            FirstName = "Customer",
            LastName = "Name",
            Email = "customer@test.com",
            PasswordHash = "hash",
            ShopId = shopId,
            Cart = new CartEntity()
        };
    }

    private static StaffEntity CreateStaff(Guid id, string firstName, string lastName)
    {
        return new StaffEntity
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = "staff@test.com",
            PasswordHash = "hash"
        };
    }

    private static InventoryItemEntity CreateInventoryItem(Guid id, int shopId, int availableAmount)
    {
        return new InventoryItemEntity
        {
            Id = id,
            ProductName = "Item",
            ProductUpc = "123456789101",
            Price = 10,
            Description = "Description",
            CategoryId = 1,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true,
            Status = InventoryItemStatusEnum.Available,
            AvailableAmount = availableAmount,
            ReservedAmount = 0,
            OnHandAmount = 100,
            ProductLocation = "A1",
            ShopId = shopId,
            Category = new CategoryEntity { Id = 1, Name = "Category", URL = "https://test.com", Visibility = true, ShopId = shopId },
            BrandId = 1,
            Brand = new BrandEntity { Id = 1, Name = "Brand", ShopId = shopId }
        };
    }

    private static async IAsyncEnumerable<InventoryItemEntity> ToAsyncEnumerable(IEnumerable<InventoryItemEntity> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
