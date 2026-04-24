using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Address.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.AddressTests.Get;

public class GetAddressByIdAndUserIdQueryHandlerTests
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly GetAddressByIdAndUserIdQueryHandler _handler;
    public GetAddressByIdAndUserIdQueryHandlerTests()
    {
        _addressRepository = Substitute.For<IAddressRepository>();
        _mapper = Substitute.For<Mapper>();

        _handler = new GetAddressByIdAndUserIdQueryHandler(_addressRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAddressResponse_WhenAddressExists()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var city = "London";
        var query = new GetAddressByIdAndUserIdQuery(addressId, userId);

        var addressEntity = new AddressEntity
        {
            Id = addressId,
            UserId = userId,
            City = city
        };

        var expectedResponse = new AddressResponse
        {
            Id = addressId,
            City = city
        };

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(addressEntity);

        _mapper.Map<AddressResponse>(addressEntity).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(addressId);
        result.City.Should().Be(city);

        await _addressRepository
            .Received(1)
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAddressDoesNotExist()
    {
        // Arrange
        var query = new GetAddressByIdAndUserIdQuery(Guid.NewGuid(), Guid.NewGuid());

        _addressRepository
            .GetAsync(Arg.Any<GetAddressByIdAndUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AddressEntity?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Address.AddressNotFoundMessage);

        _mapper.DidNotReceive().Map<AddressResponse>(Arg.Any<AddressEntity>());
    }
}