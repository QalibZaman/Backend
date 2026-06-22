using MapsterMapper;
using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.FavouriteListTests
{
    public class AddToFavouriteListCommandHandlerTests
    {
        private readonly IFavouriteListItemRepository _favouriteListItemRepository;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IStaffRepository _accountRepository;
        private readonly IMapper _mapper;

        public AddToFavouriteListCommandHandlerTests()
        {
            _favouriteListItemRepository = Substitute.For<IFavouriteListItemRepository>();
            _inventoryItemRepository = Substitute.For<IInventoryItemRepository>();
            _accountRepository = Substitute.For<IStaffRepository>();
            _mapper = MapperFactory.Create();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenInventoryItemDoesNotExist()
        {
            // Arrange
            var handler = new AddToFavouriteListCommandHandler(_favouriteListItemRepository, _inventoryItemRepository, _accountRepository, _mapper);
            var command = new AddToFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _inventoryItemRepository.ExistsAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None)); ;
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenItemAlreadyInFavourites()
        {
            // Arrange
            var handler = new AddToFavouriteListCommandHandler(_favouriteListItemRepository, _inventoryItemRepository, _accountRepository, _mapper);
            var command = new AddToFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _inventoryItemRepository.ExistsAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(true);
            _favouriteListItemRepository.ExistsAsync(Arg.Any<GetFavouriteListItemByAccountAndItemId>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldAddItem_WhenEverythingIsValid()
        {
            // Arrange
            var handler = new AddToFavouriteListCommandHandler(_favouriteListItemRepository, _inventoryItemRepository, _accountRepository, _mapper);
            var command = new AddToFavouriteListCommand(Guid.NewGuid(), Guid.NewGuid());

            _inventoryItemRepository.ExistsAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(true);
            _favouriteListItemRepository.ExistsAsync(Arg.Any<GetFavouriteListItemByAccountAndItemId>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var favouriteEntity = new FavouriteListItemEntity
            {
                AccountId = command.UserId,
                InventoryItemId = command.InventoryItemId
            };

            _favouriteListItemRepository.AddAsync(Arg.Any<FavouriteListItemEntity>(), Arg.Any<CancellationToken>())
                .Returns(favouriteEntity);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(command.UserId, result.AccountId);
            Assert.Equal(command.InventoryItemId, result.InventoryItemId);
            await _favouriteListItemRepository.Received(1).AddAsync(Arg.Any<FavouriteListItemEntity>(), Arg.Any<CancellationToken>());
        }
    }
}
