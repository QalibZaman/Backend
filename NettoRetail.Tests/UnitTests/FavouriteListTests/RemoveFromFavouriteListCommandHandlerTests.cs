using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests
{
    public class RemoveFromFavouriteListCommandHandlerTests
    {
        private readonly IFavouriteListItemRepository _repo;

        public RemoveFromFavouriteListCommandHandlerTests()
        {
            _repo = Substitute.For<IFavouriteListItemRepository>();
        }

        [Fact]
        public async Task RemoveProduct_RemovesIfPresent()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_repo);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _repo.Exists(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _repo.Received(1).DeleteAsync(
                Arg.Is<Specification<FavouriteListItemEntity>>(s => s is GetFavouriteListItemByAccountAndItemId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RemoveProduct_ThrowsWhenNotInFavourites()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_repo);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _repo.Exists(Arg.Any<Specification<FavouriteListItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            // Assert
            await _repo.DidNotReceive().DeleteAsync(
                Arg.Any<Specification<FavouriteListItemEntity>>(),
                Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task RemoveProduct_PassesCancellationToken()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_repo);
            var cts = new CancellationTokenSource();
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _repo.Exists(Arg.Any<Specification<FavouriteListItemEntity>>(), cts.Token)
                .Returns(true);

            // Act
            await handler.Handle(command, cts.Token);

            // Assert
            await _repo.Received(1).Exists(Arg.Any<Specification<FavouriteListItemEntity>>(), cts.Token);
            await _repo.Received(1).DeleteAsync(Arg.Any<Specification<FavouriteListItemEntity>>(), cts.Token);
        }
    }

}
