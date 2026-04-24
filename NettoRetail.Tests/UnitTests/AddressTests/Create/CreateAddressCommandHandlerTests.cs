using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.AddressTests.Create;

public class CreateAddressCommandHandlerTests
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly CreateAddressCommandHandler _handler;
    public CreateAddressCommandHandlerTests()
    {
        _addressRepository = Substitute.For<IAddressRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new CreateAddressCommandHandler(_addressRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldAddAddress_WhenRequestIsValid()
    {
        //Arrange

        var userId = Guid.NewGuid();
        var request = new CreateAddressRequest
        {
            CountryId = 3,
            ZipCode = "NW1 6XE",
            City = "London",
            Street = "Baker Street",
            BuildingNumber = "221B",
        };
        var command = new CreateAddressCommand(userId, request);

        var mappedEntity = new AddressEntity
        {

            CountryId = request.CountryId,
            ZipCode = request.ZipCode,
            City = request.City,
            Street = request.Street,
            BuildingNumber = request.BuildingNumber,
        };

        _mapper.Map<AddressEntity>(command.CreateAddressRequest)
            .Returns(mappedEntity);

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        await _addressRepository
            .Received(1)
            .AddAsync(Arg.Is<AddressEntity>(a =>
                a.UserId == userId &&
                a.CountryId == request.CountryId &&
                a.ZipCode == request.ZipCode &&
                a.City == request.City &&
                a.Street == request.Street &&
                a.BuildingNumber == request.BuildingNumber
            ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAssignUserIdToEntity_BeforeSaving()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateAddressRequest
        {
            CountryId = 3,
            ZipCode = "NW1 6XE",
            City = "London",
            Street = "Baker Street",
            BuildingNumber = "221B",
        };
        var command = new CreateAddressCommand(userId, request);
        var entity = new AddressEntity();

        _mapper.Map<AddressEntity>(Arg.Any<CreateAddressRequest>()).Returns(entity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        entity.UserId.Should().Be(userId);

        await _addressRepository
            .Received(1)
            .AddAsync(entity, Arg.Any<CancellationToken>());
    }
}