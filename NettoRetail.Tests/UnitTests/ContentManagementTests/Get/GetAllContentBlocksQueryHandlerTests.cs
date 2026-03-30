using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests;

public class GetAllContentBlocksQueryHandlerTests
{
    private IContentRepository _contentRepository;
    private readonly GetAllContentBlocksQueryHandler _handler;
    public GetAllContentBlocksQueryHandlerTests()
    {
        _contentRepository = Substitute.For<IContentRepository>();
        _handler = new GetAllContentBlocksQueryHandler(_contentRepository, MapperFactory.Create());
    }

    [Fact]
    public async Task GetAllContents_ValidRequest_ReturnsContentBlockResponses()
    {
        //Arrange
        int shopId = 5;
        var expectedContents = new List<ContentEntity>
        {
            new ContentEntity
            {
                Id = 1,
                ShopId = shopId,
                IsVisible = true,
                LastUpdated = DateTime.UtcNow,
                SectionType = SectionTypeEnum.PrivacyPolicy,
                UpdatedBy = "TestUser",
            },
            new ContentEntity
            {
                Id = 2,
                ShopId = shopId,
                IsVisible = false,
                LastUpdated = DateTime.UtcNow,
                SectionType = SectionTypeEnum.CookiesPolicy,
                UpdatedBy = "TestUser2",
            }
        };
        _contentRepository
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>())
            .Returns(expectedContents);

        //Act
        var query = new GetAllContentBlocksQuery(shopId);
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedContents.Count, result.Count);
        
        await _contentRepository
            .Received(1)
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>());

    }
    [Fact]
    public async Task GetAllContents_NoContents_ThrowsNotFoundException()
    {
        //Arrange
        int shopId = 5;

        _contentRepository
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContentEntity>());

        //Act
        var query = new GetAllContentBlocksQuery(shopId);

        //Assert

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Contents not found with shop id {shopId}.", ex.Message);

        await _contentRepository
            .Received(1)
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>());


    }
}
