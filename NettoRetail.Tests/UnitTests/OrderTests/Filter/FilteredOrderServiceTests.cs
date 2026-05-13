using FluentAssertions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Tests.UnitTests.OrderTests.Filter;

public class FilteredOrderServiceTests
{
    private readonly FilteredOrderService _service = new();

    [Fact]
    public void Should_Filter_By_Partial_CaseInsensitive_FirstName()
    {
        var model = new GetFilteredOrdersModel
        {
            CustomerFirstNames = ["jo"]
        };

        var spec = _service.BuildFilteredOrdersSpecification(model, 1, null, new PaginationRequest { PageNumber = 1, PageSize = 20 });
        var result = CreateOrders()
            .Where(spec.Criteria.Compile())
            .ToList();

        result.Should().ContainSingle(x => x.Customer.FirstName == "John");
    }

    [Fact]
    public void Should_Filter_By_Partial_CaseInsensitive_Email_And_Creator()
    {
        var model = new GetFilteredOrdersModel
        {
            CustomerEmails = ["GMAIL.COM"],
            Creators = ["aliyev"]
        };

        var spec = _service.BuildFilteredOrdersSpecification(model, 1, null, new PaginationRequest { PageNumber = 1, PageSize = 20 });
        var result = CreateOrders()
            .Where(spec.Criteria.Compile())
            .ToList();

        result.Should().ContainSingle(x =>
            x.Customer.Email == "john.doe@gmail.com" &&
            x.Creator == "Vugar Aliyev");
    }

    [Fact]
    public void Should_Include_Orders_Until_End_Of_EndDate()
    {
        var model = new GetFilteredOrdersModel
        {
            StartDate = new DateTime(2026, 05, 04),
            EndDate = new DateTime(2026, 05, 04)
        };

        var spec = _service.BuildFilteredOrdersSpecification(model, 1, null, new PaginationRequest { PageNumber = 1, PageSize = 20 });
        var result = CreateOrders()
            .Where(spec.Criteria.Compile())
            .ToList();

        result.Should().ContainSingle(x => x.CreationDate == new DateTime(2026, 05, 04, 23, 59, 59, DateTimeKind.Utc));
    }

    [Fact]
    public void Should_Not_Apply_Text_Filter_When_Value_Is_Whitespace()
    {
        var model = new GetFilteredOrdersModel
        {
            CustomerFirstNames = ["   "]
        };

        var spec = _service.BuildFilteredOrdersSpecification(model, 1, null, new PaginationRequest { PageNumber = 1, PageSize = 20 });
        var result = CreateOrders()
            .Where(spec.Criteria.Compile())
            .ToList();

        result.Should().HaveCount(2);
    }

    private static List<OrderEntity> CreateOrders()
    {
        return
        [
            new OrderEntity
            {
                Id = 1001,
                CustomerId = Guid.NewGuid(),
                DeliveryType = OrderDeliveryTypeEnum.Courier,
                Creator = "Vugar Aliyev",
                OrderStatus = OrderStatusEnum.New,
                CreationDate = new DateTime(2026, 05, 04, 23, 59, 59, DateTimeKind.Utc),
                ShopId = 1,
                Customer = new CustomerEntity
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@gmail.com",
                    PasswordHash = "hash",
                    Cart = new CartEntity()
                },
                OrderItems = new List<OrderItemEntity>()
            },
            new OrderEntity
            {
                Id = 1002,
                CustomerId = Guid.NewGuid(),
                DeliveryType = OrderDeliveryTypeEnum.SelfDelivery,
                Creator = "Leyla Mammadova",
                OrderStatus = OrderStatusEnum.Processing,
                CreationDate = new DateTime(2026, 05, 05, 10, 0, 0, DateTimeKind.Utc),
                ShopId = 1,
                Customer = new CustomerEntity
                {
                    FirstName = "Anna",
                    LastName = "Smith",
                    Email = "anna@company.az",
                    PasswordHash = "hash",
                    Cart = new CartEntity()
                },
                OrderItems = new List<OrderItemEntity>()
            }
        ];
    }
}
