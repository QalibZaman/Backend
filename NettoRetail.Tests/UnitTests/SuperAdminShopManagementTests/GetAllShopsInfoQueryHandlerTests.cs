using System.Linq.Expressions;
using MapsterMapper;
using NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.SuperAdminShopManagementTests;

public class GetAllShopsInfoQueryHandlerTests
{
    private readonly IShopRepository _shopRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IMapper _mapper;

    public GetAllShopsInfoQueryHandlerTests()
    {
        _shopRepository = Substitute.For<IShopRepository>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _mapper = MapperFactory.Create();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmphtyPaginetedResponse_WhenNoShopsExist() 
    {
        _shopRepository
            .GetAllPaginatedAsync(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
            .Returns((new List<ShopEntity>(), 0));

        var handler = new GetAllShopsInfoQueryHandler(_shopRepository, _staffRepository, _mapper);
        var query = new GetAllShopsInfoQuery(new PaginationRequest { PageNumber = 1, PageSize = 10 });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalAmount);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedShops_WithAdminAndSubscriptionData()
    {
        // Arrange
        var shops = new List<ShopEntity>
        {
            CreateShop(10, "Downtown", "Premium"),
            CreateShop(20, "Uptown", "Basic")
        };

        var admins = new List<StaffEntity>
        {
            CreateAdmin(10, "Alice", "Meyer", "alice@shop.com", "+35111111"),
            CreateAdmin(20, "Bob", "Brown", "bob@shop.com", "+35122222")
        };

        Specification<ShopEntity>? capturedShopSpecification = null;

        _shopRepository
            .GetAllPaginatedAsync(
                Arg.Do<Specification<ShopEntity>>(spec => capturedShopSpecification = spec),
                Arg.Any<CancellationToken>())
            .Returns((shops, shops.Count));

        _staffRepository
            .GetManyBySpecAsync(Arg.Any<Specification<StaffEntity>>(), Arg.Any<CancellationToken>())
            .Returns(admins);

        var handler = new GetAllShopsInfoQueryHandler(_shopRepository, _staffRepository, _mapper);
        var query = new GetAllShopsInfoQuery(new PaginationRequest { PageNumber = 1, PageSize = 10 });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalAmount);
        Assert.Equal(2, result.Items.Count);

        var firstShop = result.Items.Single(x => x.Id == 10);
        Assert.Equal("Downtown", firstShop.Name);
        Assert.Equal("Premium", firstShop.Subscription);
        Assert.Equal("Alice Meyer", firstShop.AdminFullName);
        Assert.Equal("alice@shop.com", firstShop.AdminEmail);
        Assert.Equal("+35111111", firstShop.AdminPhoneNumber);

        var secondShop = result.Items.Single(x => x.Id == 20);
        Assert.Equal("Basic", secondShop.Subscription);
        Assert.Equal("Bob Brown", secondShop.AdminFullName);

        Assert.NotNull(capturedShopSpecification);
        Assert.Equal(0, capturedShopSpecification!.Skip);
        Assert.Equal(10, capturedShopSpecification.Take);

        var includeNames = capturedShopSpecification.Includes.Select(GetMemberName).ToList();
        Assert.Contains(nameof(ShopEntity.SubscriptionPlan), includeNames);
    }

    [Fact]
    public async Task Handle_ShouldQueryAdminsInBulk_AndAvoidPerShopLookups()
    {
        // Arrange
        var shops = new List<ShopEntity>
        {
            CreateShop(1, "North", "Premium"),
            CreateShop(2, "South", "Basic"),
            CreateShop(3, "West", "Basic")
        };

        _shopRepository
            .GetAllPaginatedAsync(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
            .Returns((shops, shops.Count));

        _staffRepository
            .GetManyBySpecAsync(Arg.Any<Specification<StaffEntity>>(), Arg.Any<CancellationToken>())
            .Returns(new List<StaffEntity>());

        var handler = new GetAllShopsInfoQueryHandler(_shopRepository, _staffRepository, _mapper);

        // Act
        var result = await handler.Handle(
            new GetAllShopsInfoQuery(new PaginationRequest { PageNumber = 1, PageSize = 20 }),
            CancellationToken.None);

        // Assert
        await _staffRepository.Received(1)
            .GetManyBySpecAsync(Arg.Any<Specification<StaffEntity>>(), Arg.Any<CancellationToken>());

        await _staffRepository.DidNotReceive()
            .GetAsync(Arg.Any<Specification<StaffEntity>>(), Arg.Any<CancellationToken>());

        Assert.All(result.Items, shop =>
        {
            Assert.Equal(string.Empty, shop.AdminFullName);
            Assert.Equal(string.Empty, shop.AdminEmail);
            Assert.Equal(string.Empty, shop.AdminPhoneNumber);
        });
    }

    private static ShopEntity CreateShop(int id, string name, string subscriptionName)
    {
        return new ShopEntity
        {
            Id = id,
            Name = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            PhoneNumber = "+3510000000",
            Url = $"https://{name.ToLowerInvariant()}.example.com",
            IsActive = true,
            SubscriptionPlanId = id,
            SubscriptionPlan = new SubscriptionPlanEntity
            {
                Id = id,
                Type = SubscriptionTypeEnum.Free,
                Name = subscriptionName,
                Description = "plan",
                MonthlyPrice = 100,
                Moto = "motto",
                SubscriptionId = $"sub_{id}"
            }
        };
    }

    private static StaffEntity CreateAdmin(int shopId, string firstName, string lastName, string email, string phone)
    {
        return new StaffEntity
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = "hash",
            RoleId = 1,
            ShopId = shopId,
            PhoneNumber = phone
        };
    }

    private static string GetMemberName(Expression<Func<ShopEntity, object>> expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression unary when unary.Operand is MemberExpression member => member.Member.Name,
            _ => throw new InvalidOperationException("Unsupported include expression")
        };
    }
}
