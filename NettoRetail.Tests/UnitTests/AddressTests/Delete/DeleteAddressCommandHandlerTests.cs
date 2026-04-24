using FluentAssertions;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using System.Data;

namespace NettoRetail.Tests.UnitTests.AddressTests.Delete;

public class DeleteAddressCommandHandlerTests
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _transaction;
    private readonly DeleteAddressCommandHandler _handler;
    public DeleteAddressCommandHandlerTests()
    {
        _addressRepository = Substitute.For<IAddressRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new DeleteAddressCommandHandler(_addressRepository, _unitOfWork);

    }

    [Fact]
    public async Task Handle_ShouldDeleteAddress_WhenValidRequestAndNotDefault()
    {
        // Arrange
        var address = new AddressEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsDefault = false };
        var command = new DeleteAddressCommand(address.Id, address.UserId);

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(address);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _addressRepository.Received(1).DeleteAsync(address, Arg.Any<CancellationToken>());
        await _addressRepository.DidNotReceive().GetFallbackAddressAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldPromoteFallback_WhenDeletedAddressWasDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressToDelete = new AddressEntity { Id = Guid.NewGuid(), UserId = userId, IsDefault = true };
        var fallbackAddress = new AddressEntity { Id = Guid.NewGuid(), UserId = userId, IsDefault = false };
        var command = new DeleteAddressCommand(addressToDelete.Id, userId);

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(addressToDelete);

        _addressRepository
            .GetFallbackAddressAsync(userId, addressToDelete.Id, Arg.Any<CancellationToken>())
            .Returns(fallbackAddress);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        fallbackAddress.IsDefault.Should().BeTrue();
        await _addressRepository.Received(1).UpdateAsync(fallbackAddress, Arg.Any<CancellationToken>());
        await _addressRepository.Received(1).DeleteAsync(addressToDelete, Arg.Any<CancellationToken>());

        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAddressDoesNotExist()
    {
        // Arrange
        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AddressEntity?)null);

        var command = new DeleteAddressCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage(HandlerConstants.Address.AddressNotFoundMessage);

        _unitOfWork.DidNotReceive().BeginTransaction();
        await _addressRepository.DidNotReceive().DeleteAsync(Arg.Any<AddressEntity>(), Arg.Any<CancellationToken>());
    }
}