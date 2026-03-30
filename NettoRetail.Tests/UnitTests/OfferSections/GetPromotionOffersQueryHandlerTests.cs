using MapsterMapper;
using NettoRetail.Application.CQRS.OfferSections.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OfferSections;

public class GetPromotionOffersQueryHandlerTests
{
    private readonly IOfferRepository _offerRepo;
    private readonly IMapper _mapper;
    public GetPromotionOffersQueryHandlerTests()
    {
        _offerRepo = Substitute.For<IOfferRepository>();
        _mapper = MapperFactory.Create();
    }
    private static OfferEntity BuildOffer()
    {
        var inv1 = Guid.NewGuid();
        var inv2 = Guid.NewGuid();
        var firstItem = Substitute.For<InventoryItemEntity>();
        var secondItem = Substitute.For<InventoryItemEntity>();
        return new OfferEntity
        {
            Id = 1,
            ShopId = 5,
            Name = "Hot offers",
            IsVisible = true,
            SelectionType = SelectionTypeEnum.Static,
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = "tester",
            Products = new List<OffersProductEntity>
            {
                new OffersProductEntity { Id = 101, OfferId = 1, InventoryItemId = inv1, DisplayOrder = 2, InventoryItem = firstItem},
                new OffersProductEntity { Id = 102, OfferId = 1, InventoryItemId = inv2, DisplayOrder = 1, InventoryItem = secondItem}
            }
        };
    }

    [Fact]
    public async Task Handle_Returns_Data_When_Offers_Exist()
    {
        // Arrange
        var offers = new List<OfferEntity> { BuildOffer() };

        // Match the signature your handler calls (spec + ct)
        _offerRepo
            .GetManyBySpecAsync(Arg.Any<Specification<OfferEntity>>(), Arg.Any<CancellationToken>())
            .Returns(offers);

        var handler = new GetPromotionOffersQueryHandler(_offerRepo, _mapper);
        var request = new GetPromotionOffersQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_Returns_Empty_When_No_Offers()
    {
        // Arrange
        _offerRepo
            .GetManyBySpecAsync(Arg.Any<Specification<OfferEntity>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OfferEntity>());

        var handler = new GetPromotionOffersQueryHandler(_offerRepo, _mapper);
        var request = new GetPromotionOffersQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
