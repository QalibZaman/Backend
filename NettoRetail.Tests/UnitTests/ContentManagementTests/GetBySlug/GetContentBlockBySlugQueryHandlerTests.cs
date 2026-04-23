using FluentAssertions;
using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.GetBySlug;

public class GetContentBlockBySlugQueryHandlerTests
{
    private readonly IContentRepository _contentRepository;
    private readonly GetContentBlockBySlugQueryHandler _handler;

    public GetContentBlockBySlugQueryHandlerTests()
    {
        _contentRepository = Substitute.For<IContentRepository>();
        _handler = new GetContentBlockBySlugQueryHandler(_contentRepository, MapperFactory.Create());
    }

    [Fact]
    public async Task Handle_WhenBlockExists_ReturnsMappedResponse()
    {
        var entity = new ContentEntity
        {
            Id = 7,
            ShopId = 1,
            SectionType = SectionTypeEnum.OurStory,
            Content = "<p>Story</p>",
            IsVisible = true,
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = "Admin User",
        };

        _contentRepository
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(entity);

        var result = await _handler.Handle(new GetContentBlockBySlugQuery("1-OurStory"), CancellationToken.None);

        result.Id.Should().Be(7);
        result.SectionType.Should().Be(SectionTypeEnum.OurStory.ToString());
        result.Content.Should().Be("<p>Story</p>");
        result.IsVisible.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBlockDoesNotExist_ReturnsEmptyResponse()
    {
        _contentRepository
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns((ContentEntity?)null);

        var result = await _handler.Handle(new GetContentBlockBySlugQuery("1-PrivacyPolicy"), CancellationToken.None);

        result.Id.Should().Be(0);
        result.SectionType.Should().Be(SectionTypeEnum.PrivacyPolicy.ToString());
        result.Content.Should().BeEmpty();
        result.IsVisible.Should().BeFalse();
        result.LastUpdated.Should().BeNull();
        result.UpdatedBy.Should().BeNull();
    }
}
