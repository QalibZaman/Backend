using FluentAssertions;
using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests;

public class GetContentBlockByIdQueryHanderTests
{
    private readonly IContentRepository _contentRepository;
    private readonly GetContentBlockByIdQueryHandler _handler;
    public GetContentBlockByIdQueryHanderTests()
    {
        _contentRepository = Substitute.For<IContentRepository>();
        _handler = new GetContentBlockByIdQueryHandler(_contentRepository, MapperFactory.Create());
    }

    [Fact]
    public async Task GetUserById_ValidRequest_ReturnsContentBlockResponse()
    {
        //Arrange 
        int id = 1;
        int shopId = 5;
        var expectedResponse = new ContentEntity
        {
            Id = id,
            ShopId = shopId,
            IsVisible = true,
            LastUpdated = DateTime.UtcNow,
            SectionType = SectionTypeEnum.PrivacyPolicy,
            UpdatedBy = "TestUser",
        };

        _contentRepository
            .GetAsync(Arg.Any<GetContentBlockByShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ContentEntity?>(expectedResponse));

        //Act
        var query = new GetContentBlockByIdQuery(id, shopId);

        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Content, result.Content);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.IsVisible, result.IsVisible);
        Assert.Equal(expectedResponse.LastUpdated, result.LastUpdated);
        Assert.Equal(expectedResponse.UpdatedBy, result.UpdatedBy);
        Assert.Equal(expectedResponse.SectionType.ToString(), result.SectionType);

        await _contentRepository
            .Received(1)
            .GetAsync(Arg.Any<GetContentBlockByShopIdSpecification>(), Arg.Any<CancellationToken>());


    }

    [Fact]
    public async Task GetUserById_ContentNotFound_ThrowsNotFoundException()
    {
        //Arrange
        int id = 1;
        int shopId = 5;
        _contentRepository.GetAsync(Arg.Any<GetContentBlockByShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ContentEntity?>(null));

        var query = new GetContentBlockByIdQuery(id, shopId);
        //Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Content not found", ex.Message);

        await _contentRepository
            .Received(1)
            .GetAsync(Arg.Any<GetContentBlockByShopIdSpecification>(), Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task GetUserById_EmptyContent_IsReturnedAsInvisible()
    {
        int id = 1;
        int shopId = 5;
        var content = new ContentEntity
        {
            Id = id,
            ShopId = shopId,
            Content = "<p><br></p>",
            IsVisible = false,
            LastUpdated = DateTime.UtcNow,
            SectionType = SectionTypeEnum.PrivacyPolicy,
            UpdatedBy = "TestUser",
        };

        _contentRepository
            .GetAsync(Arg.Any<GetContentBlockByShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ContentEntity?>(content));

        var result = await _handler.Handle(new GetContentBlockByIdQuery(id, shopId), CancellationToken.None);

        result.IsVisible.Should().BeFalse();
    }
}
