using MapsterMapper;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests.GetOrderStatusHistory;

public class GetOrderStatusHistoryQueryHandlerTests
{
    private readonly IOrderStatusHistoryRepository _historyRepository;
    private readonly IMapper _mapper;
    private readonly GetOrderStatusHistoryQueryHandler _handler;
    public GetOrderStatusHistoryQueryHandlerTests()
    {
        _historyRepository = Substitute.For<IOrderStatusHistoryRepository>();
        _mapper = MapperFactory.Create();
        _handler = new GetOrderStatusHistoryQueryHandler(_historyRepository, _mapper);
    }

    [Fact]
    public async Task Handler_ShouldReturnEmptyList_WhenNoHistoryExists()
    {
        // Arrange
        var orderId = 133;
        var shopId = 1;

        _historyRepository
            .GetOrderHistoryIncludingAccount(orderId, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<OrderStatusHistoryEntity>());

        var query = new GetOrderStatusHistoryQuery(orderId, shopId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        await _historyRepository.Received(1).GetOrderHistoryIncludingAccount(orderId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handler_ShouldReturnMappedHistory_WhenHistoryExists()
    {
        // Arrange
        var orderId = 133;
        var shopId = 1;
        var historyEntities = BuildTestHistory(orderId);

        _historyRepository
            .GetOrderHistoryIncludingAccount(orderId, Arg.Any<CancellationToken>())
            .Returns(historyEntities);

        var query = new GetOrderStatusHistoryQuery(orderId, shopId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(historyEntities.Count, resultList.Count);

        Assert.Equal(historyEntities.First().Comment, resultList.First().Comment);
        Assert.Equal(historyEntities.First().NewStatus.ToString(), resultList.First().Status);

        await _historyRepository.Received(1).GetOrderHistoryIncludingAccount(orderId, Arg.Any<CancellationToken>());
    }
    private static List<OrderStatusHistoryEntity> BuildTestHistory(int orderId)
    {
        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "test@gmail.com",
            PasswordHash = "hashedPassword"
        };

        return new List<OrderStatusHistoryEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                OldStatus = OrderStatusEnum.New,
                NewStatus = OrderStatusEnum.Processing,
                Comment = "Confirmed by warehouse",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedBy = staff
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                OldStatus = OrderStatusEnum.New,
                NewStatus = OrderStatusEnum.InProgress,
                Comment = "Payment received",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedBy = staff
            }
        };
    }
}
