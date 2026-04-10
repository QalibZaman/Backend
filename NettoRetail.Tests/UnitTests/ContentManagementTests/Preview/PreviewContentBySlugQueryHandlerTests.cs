using FluentAssertions;
using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Content;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.Preview;

public class PreviewContentBySlugQueryHandlerTests
{
    private readonly PreviewContentBySlugQueryHandler _handler = new();

    [Fact]
    public async Task Handle_ValidPreview_ReturnsSanitizedContent()
    {
        var result = await _handler.Handle(
            new PreviewContentBySlugQuery(
                "1-OurShop",
                new UpdateContentRequest
                {
                    Content = "<p>Hello</p><script>alert('x')</script>",
                    IsVisible = true,
                }),
            CancellationToken.None);

        result.ShopId.Should().Be(1);
        result.SectionType.Should().Be("OurShop");
        result.Content.Should().Be("<p>Hello</p>");
        result.IsVisible.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyPreview_ThrowsBadRequest()
    {
        var action = () => _handler.Handle(
            new PreviewContentBySlugQuery(
                "1-OurShop",
                new UpdateContentRequest
                {
                    Content = "<p><br></p>",
                    IsVisible = true,
                }),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<BadRequestException>(action);
        ex.Message.Should().Be("\"Field name\" cannot be empty.");
    }
}
