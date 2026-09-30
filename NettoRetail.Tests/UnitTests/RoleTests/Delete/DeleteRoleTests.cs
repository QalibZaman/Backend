using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Controllers.V1;
using NettoRetail.Application.CQRS.Roles.Commands;

namespace NettoRetail.Tests.UnitTests.RoleTests.Delete;

public class DeleteRoleTests
{
    private class FakeMediator : IMediator
    {
        public object? LastSentCommand { get; private set; }
        public int SendCallCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastSentCommand = request;
            SendCallCount++;
            return Task.FromResult(default(TResponse)!);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            LastSentCommand = request;
            SendCallCount++;
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            LastSentCommand = request;
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

    [Fact]
    public async Task DeleteRole_ShouldReturnNoContent_WhenDeletionIsSuccessful()
    {
        // Arrange
        var fakeMediator = new FakeMediator();
        var controller = new RoleController(fakeMediator);
        int roleId = 1;

        // Act
        var result = await controller.DeleteRole(roleId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, fakeMediator.SendCallCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(999)]
    public async Task DeleteRole_ShouldSendCorrectCommandWithRoleId(int roleId)
    {
        // Arrange
        var fakeMediator = new FakeMediator();
        var controller = new RoleController(fakeMediator);

        // Act
        await controller.DeleteRole(roleId, CancellationToken.None);

        // Assert
        Assert.NotNull(fakeMediator.LastSentCommand);
        var command = Assert.IsType<DeleteRoleCommand>(fakeMediator.LastSentCommand);
        Assert.NotNull(command);
    }
}