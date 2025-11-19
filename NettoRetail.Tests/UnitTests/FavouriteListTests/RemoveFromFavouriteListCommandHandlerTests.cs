using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests
{
    public class RemoveFromFavouriteListCommandHandlerTests
    {
        private readonly IFavouriteListItemRepository _favouriteListItemRepository;

        public RemoveFromFavouriteListCommandHandlerTests()
        {
            _favouriteListItemRepository = Substitute.For<IFavouriteListItemRepository>();
        }

        [Fact(DisplayName = "Removing a product from Favorites succeeds (idempotent)")]
        public async Task RemoveProduct_RemovesIfPresent_Idempotent()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository);
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var command = new RemoveFromFavouriteListCommand(userId, itemId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _favouriteListItemRepository.Received(1).DeleteManyBySpecAsync(
                Arg.Is<Specification<FavouriteListItemEntity>>(s => s is GetFavouriteListItemByAccountAndItemId),
                Arg.Any<CancellationToken>());
        }

        [Fact(DisplayName = "Second remove is safe when item already removed")]
        public async Task RemoveProduct_IsSafeWhenAlreadyRemoved()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _favouriteListItemRepository.Received(1).DeleteManyBySpecAsync(
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                Arg.Any<CancellationToken>());
        }

        [Fact(DisplayName = "Operational – Cancellation flows through to repository")]
        public async Task RemoveProduct_RespectsOperationCancellation()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository);
            var cts = new CancellationTokenSource();
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            await handler.Handle(command, cts.Token);

            // Assert
            await _favouriteListItemRepository.Received(1).DeleteManyBySpecAsync(
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                cts.Token);
        }
    }
}
