using FluentAssertions;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using System.Data;

namespace NettoRetail.Tests.UnitTests.AddressTests.Update;

public class UpdateAddressCommandHandlerTests
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly UpdateAddressCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbTransaction _dbTransaction;
    public UpdateAddressCommandHandlerTests()
    {
        _addressRepository = Substitute.For<IAddressRepository>();
        _mapper = Substitute.For<IMapper>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dbTransaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_dbTransaction);


        _handler = new UpdateAddressCommandHandler(_addressRepository, _mapper, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAddress_WhenAddressExistsAndBelongsToUser()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateAddressRequest 
        {
            CountryId = 1,
            ZipCode = "12345",
            BuildingNumber = "10A",
            City = "Berlin", 
            Street = "Updated St",
            
        };
        var command = new UpdateAddressCommand(addressId, userId, updateRequest);

        var existingAddress = new AddressEntity
        {
            Id = addressId,
            UserId = userId,
            City = "Old City"
        };

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existingAddress);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mapper.Received(1).Map(command.UpdateAddress, existingAddress);
        _unitOfWork.Received(1).BeginTransaction();
        _dbTransaction.Received(1).Commit();

        await _addressRepository
            .Received(1)
            .UpdateAsync(existingAddress, Arg.Any<CancellationToken>());

        await _addressRepository
            .Received(1)
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAddressDoesNotExist()
    {
        // Arrange

        var request = new UpdateAddressRequest
        {
            CountryId = 1,
            ZipCode = "12345",
            BuildingNumber = "10A",
            City = "Berlin",
            Street = "Updated St",
        };

        var command = new UpdateAddressCommand(Guid.NewGuid(), Guid.NewGuid(), request);

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AddressEntity?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Address.AddressNotFoundMessage);

        _mapper.DidNotReceiveWithAnyArgs().Map(Arg.Any<object>(), Arg.Any<object>());
        await _addressRepository.DidNotReceive().UpdateAsync(Arg.Any<AddressEntity>(), Arg.Any<CancellationToken>());
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenSettingDefaultWithOnlyOneAddress()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var updateRequest = new UpdateAddressRequest 
        {
            CountryId = 1,
            ZipCode= "1234",
            City = "Test City",
            BuildingNumber = "10A",
            Street = "Test Street",
            IsDefault = true 
        };
        var command = new UpdateAddressCommand(addressId, Guid.NewGuid(), updateRequest);

        var existingAddress = new AddressEntity
        {
            Id = addressId,
            IsDefault = false,
            User = new BaseUserEntity 
            {
                FirstName = "Test",
                LastName = "Test",
                Email = "test@gmail.com",
                PasswordHash = "hashedpassword",
                Addresses = new List<AddressEntity> { new() } 
            }
        };

        _addressRepository.GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existingAddress);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(HandlerConstants.Address.AtLeastTwoAddressesRequiredMessage);

        _unitOfWork.Received().BeginTransaction();
        _dbTransaction.DidNotReceive().Commit();
    }
}