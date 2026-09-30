using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Controllers.V1;
using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.Tests.UnitTests.RoleTests.Get;

public class GetRoleTests
{
    private class FakeMediator : IMediator
    {
        public object? LastSentQuery { get; private set; }
        public int SendCallCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastSentQuery = request;
            SendCallCount++;
            return Task.FromResult(default(TResponse)!);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            LastSentQuery = request;
            SendCallCount++;
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            LastSentQuery = request;
            SendCallCount++;
            return Task.FromResult<object?>(null);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return Task.CompletedTask;
        }
    }

    private static RoleController CreateControllerWithShopContext(IMediator mediator, int shopId = 1)
    {
        var claims = new[] { new Claim("shop_id", shopId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        return new RoleController(mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    [Fact]
    public async Task GetAllRoles_ShouldReturnOkResult_WhenCalled()
    {
        // Arrange
        var fakeMediator = new FakeMediator();
        var controller = CreateControllerWithShopContext(fakeMediator, shopId: 1);
        var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await controller.GetAllRoles(paginationRequest, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, fakeMediator.SendCallCount);
        Assert.IsType<GetRolesQuery>(fakeMediator.LastSentQuery);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(999)]
    public async Task GetRolesById_ShouldSendCorrectQueryWithRoleId(int roleId)
    {
        // Arrange
        var fakeMediator = new FakeMediator();
        var controller = CreateControllerWithShopContext(fakeMediator, shopId: 1);

        // Act
        var result = await controller.GetRolesById(roleId, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, fakeMediator.SendCallCount);
        Assert.IsType<GetRolesQueryById>(fakeMediator.LastSentQuery);
    }

    [Fact]
    public async Task GetRolesByName_ShouldReturnOkResult_WhenRoleExists()
    {
        // Arrange
        var fakeMediator = new FakeMediator();
        var controller = CreateControllerWithShopContext(fakeMediator, shopId: 1);
        string roleName = "Admin";

        // Act
        var result = await controller.GetRolesByName(roleName, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, fakeMediator.SendCallCount);
        Assert.IsType<GetRolesQueryByName>(fakeMediator.LastSentQuery);
    }
}