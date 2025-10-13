using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests
{
    public class RemoveFromFavouriteListCommandHandlerTests
    {
        private readonly IFavouriteListItemRepository _favouriteListItemRepository;
        private readonly IAccountRepository _accountRepository;

        public RemoveFromFavouriteListCommandHandlerTests()
        {
            _favouriteListItemRepository = Substitute.For<IFavouriteListItemRepository>();
            _accountRepository = Substitute.For<IAccountRepository>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository, _accountRepository);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _accountRepository.Exists(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenFavouriteItemDoesNotExist()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository, _accountRepository);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _accountRepository.Exists(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(true);
            _favouriteListItemRepository.GetAsync(Arg.Any<GetFavouriteListItemByAccountAndItemId>(), Arg.Any<CancellationToken>())
                .ReturnsNull();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCallDelete_WhenItemExists()
        {
            // Arrange
            var handler = new RemoveFromFavouriteListCommandHandler(_favouriteListItemRepository, _accountRepository);
            var command = new RemoveFromFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _accountRepository.Exists(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(true);

            var favouriteItem = new FavouriteListItemEntity
            {
                AccountId = command.UserId,
                InventoryItemId = command.InventoryItemId
            };

            _favouriteListItemRepository.GetAsync(Arg.Any<GetFavouriteListItemByAccountAndItemId>(), Arg.Any<CancellationToken>())
                .Returns(favouriteItem);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _favouriteListItemRepository.Received(1).DeleteAsync(favouriteItem, Arg.Any<CancellationToken>());
        }
    }
}
