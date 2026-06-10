using NettoRetail.Application.CQRS.Contents.Queries;
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
        Assert.Equal(Enum.GetValues<SectionTypeEnum>().Length, result.Count);
        Assert.Contains(result, x => x.SectionType == SectionTypeEnum.PrivacyPolicy.ToString() && x.Id == 1);
        Assert.Contains(result, x => x.SectionType == SectionTypeEnum.CookiesPolicy.ToString() && x.Id == 2);
        
        await _contentRepository
            .Received(1)
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>());

    }
    [Fact]
    public async Task GetAllContents_NoContents_ReturnsAllEmptySections()
    {
        int shopId = 5;

        _contentRepository
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContentEntity>());

        var result = await _handler.Handle(new GetAllContentBlocksQuery(shopId), CancellationToken.None);

        Assert.Equal(Enum.GetValues<SectionTypeEnum>().Length, result.Count);
        Assert.All(result, x =>
        {
            Assert.False(x.IsVisible);
            Assert.Equal(0, x.Id);
            Assert.Equal(string.Empty, x.Content);
            Assert.Null(x.LastUpdated);
            Assert.Null(x.UpdatedBy);
        });

        await _contentRepository
            .Received(1)
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllContents_EmptyContent_IsReturnedAsInvisible()
    {
        int shopId = 5;
        var contents = new List<ContentEntity>
        {
            new ContentEntity
            {
                Id = 1,
                ShopId = shopId,
                Content = "<p><br></p>",
                IsVisible = false,
                LastUpdated = DateTime.UtcNow,
                SectionType = SectionTypeEnum.OurStory,
                UpdatedBy = "TestUser",
            }
        };

        _contentRepository
            .GetAllBySpecAsync(Arg.Any<GetContentByShopSpecification>(), Arg.Any<CancellationToken>())
            .Returns(contents);

        var result = await _handler.Handle(new GetAllContentBlocksQuery(shopId), CancellationToken.None);

        var ourStoryBlock = result.Single(x => x.SectionType == SectionTypeEnum.OurStory.ToString());
        Assert.False(ourStoryBlock.IsVisible);
    }
}
