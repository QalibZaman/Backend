using FluentAssertions;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetFilteredOrders;

public class GetFilteredOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFilteredOrderService _filteredOrderService;
    private readonly GetFilteredOrdersQueryHandler _handler;

    public GetFilteredOrdersQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _filteredOrderService = Substitute.For<IFilteredOrderService>();

        var mapper = MapperFactory.Create();

        _handler = new GetFilteredOrdersQueryHandler(_orderRepository, _filteredOrderService, mapper);
    }
    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponse_WhenOrdersExist()
    {
        // Arrange
        var model = new GetFilteredOrdersModel();
        var pagination = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var query = new GetFilteredOrdersQuery(model, 1, "search", pagination);

        var mockSpec = new GetOrdersByShopIdSpecification(1);
        var orders = new List<OrderEntity>
        {
            new()
            {
                Id = 1,
                Delivery = new DeliveryEntity { DeliveryType = OrderDeliveryTypeEnum.SelfDelivery },
                CreationDate = DateTime.Now,
                Creator = "Test",
                OrderStatus = OrderStatusEnum.InProgress,
                CustomerId = Guid.NewGuid(),
                Customer = new CustomerEntity { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Test",  Email = "test@gmail.com", PasswordHash = "hashedPassword" }
            }
        };

        _filteredOrderService
            .BuildFilteredOrdersSpecification(query.Model, query.ShopId, query.SearchTerm, query.PageModel)
            .Returns(mockSpec);

        _orderRepository
            .GetAllPaginatedAsync(mockSpec, Arg.Any<CancellationToken>())
            .Returns((orders, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be(1);

        _filteredOrderService.Received(1)
            .BuildFilteredOrdersSpecification(query.Model, query.ShopId, query.SearchTerm, query.PageModel);

        await _orderRepository.Received(1)
            .GetAllPaginatedAsync(mockSpec, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResponse_WhenRepositoryReturnsNoData()
    {
        // Arrange
        var query = new GetFilteredOrdersQuery(new(), 1, null, new());
        var mockSpec = new GetOrdersByShopIdSpecification(1);

        _filteredOrderService
            .BuildFilteredOrdersSpecification(Arg.Any<GetFilteredOrdersModel>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<PaginationRequest>())
            .Returns(mockSpec);

        _orderRepository
            .GetAllPaginatedAsync(mockSpec, Arg.Any<CancellationToken>())
            .Returns((new List<OrderEntity>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldUseSpecificationFromService_ExactlyAsReturned()
    {
        // Arrange
        var query = new GetFilteredOrdersQuery(new(), 1, "test", new());
        var differentSpec = new GetOrdersByShopIdSpecification(3);

        _filteredOrderService
            .BuildFilteredOrdersSpecification(Arg.Any<GetFilteredOrdersModel>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<PaginationRequest>())
            .Returns(differentSpec);

        _orderRepository
            .GetAllPaginatedAsync(differentSpec, Arg.Any<CancellationToken>())
            .Returns((new List<OrderEntity>(), 0));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _orderRepository.Received(1).GetAllPaginatedAsync(differentSpec, Arg.Any<CancellationToken>());
    }
}
