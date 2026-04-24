using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Address.Queries;
using NettoRetail.Application.Models.Response.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.AddressTests.GetAll;

public class GetAddressesQueryHandlerTests
{
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly GetAddressesQueryHandler _handler;
    public GetAddressesQueryHandlerTests()
    {
        _addressRepository = Substitute.For<IAddressRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new GetAddressesQueryHandler(_addressRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfAddressResponses_WhenAddressesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAddressesQuery(userId);

        var addressEntities = new List<AddressEntity>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, City = "London" },
            new() { Id = Guid.NewGuid(), UserId = userId, City = "Manchester" }
        };

        var expectedResponses = new List<AddressResponse>
        {
            new() { Id = addressEntities[0].Id, City = "London" },
            new() { Id = addressEntities[1].Id, City = "Manchester" }
        };

        _addressRepository
            .GetManyBySpecAsync(Arg.Any<GetAddressesByUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(addressEntities);

        _mapper.Map<IList<AddressResponse>>(addressEntities).Returns(expectedResponses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].City.Should().Be("London");

        await _addressRepository
            .Received(1)
            .GetManyBySpecAsync(Arg.Any<GetAddressesByUserIdSpecification>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAddressesFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAddressesQuery(userId);
        var emptyList = new List<AddressEntity>();

        _addressRepository
            .GetManyBySpecAsync(Arg.Any<GetAddressesByUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(emptyList);

        _mapper.Map<IList<AddressResponse>>(emptyList).Returns(new List<AddressResponse>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}