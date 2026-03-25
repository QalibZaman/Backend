using NettoRetail.Application.CQRS.Contents.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.Update;


public class UpdateContentBySlugCommandHandlerTests
{
    private readonly IContentRepository _contentRepositoryMock;
    private readonly IStaffRepository _staffRepositoryMock;
    private UpdateContentBySlugCommandHandler _handler;
    public UpdateContentBySlugCommandHandlerTests()
    {
        _contentRepositoryMock = Substitute.For<IContentRepository>();
        _staffRepositoryMock = Substitute.For<IStaffRepository>();
        _handler = new UpdateContentBySlugCommandHandler(_contentRepositoryMock, _staffRepositoryMock, MapperFactory.Create());
    }
    [Fact]
    public async Task Handle_ValidUpdate()
    {
        //Arrange
        Guid AuthorId = Guid.NewGuid();
        string slug = "1-OurStory";
        var staff = CreateStaff(AuthorId);

        var newSlug = ContentSlugParser.ParseContentSlug(slug);

        var contentBlock = new ContentEntity
        {
            Id = 1,
            ShopId = newSlug.ShopId,
            SectionType = newSlug.SectionType,
            IsVisible = true,
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = staff.Email
        };
        MockStaffRepository(staff);

        MockContentRepository(contentBlock);

        _contentRepositoryMock
            .UpdateAsync(contentBlock, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(contentBlock));
        var updateModel = CreateUpdateModel();

        //Act
        var request = new UpdateContentBySlugCommand(slug, AuthorId, updateModel);
        await _handler.Handle(request, CancellationToken.None);

        //Assert
        await _staffRepositoryMock
            .Received(1)
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>());

        await _contentRepositoryMock
            .Received(1)
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>());
        await _contentRepositoryMock
            .Received(1)
            .UpdateAsync(contentBlock, Arg.Any<CancellationToken>());

    }
    [Fact]
    public async Task Handle_AuthorNotFound_ReturnsBadRequest()
    {
        //Arrange
        Guid AuthorId = Guid.NewGuid();
        string slug = "1-OurStory";

        var updateModel = CreateUpdateModel();

        MockStaffRepository(null);

        //Act
        var request = new UpdateContentBySlugCommand(slug, AuthorId, updateModel);

        //Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal("Couldn't find account record with given user identifier", ex.Message);

        await _staffRepositoryMock
            .Received(1)
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>());
        await _contentRepositoryMock
            .Received(0)
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>());
        await _contentRepositoryMock
            .Received(0)
            .AddAsync(Arg.Any<ContentEntity>(), Arg.Any<CancellationToken>());

        await _contentRepositoryMock
            .Received(0)
            .UpdateAsync(Arg.Any<ContentEntity>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ContentBlockNotFound_CreatesNewContentBlock()
    {
        //Arrange
        Guid AuthorId = Guid.NewGuid();
        string slug = "1-OurStory";
        var updateModel = CreateUpdateModel();

        var staff = CreateStaff(AuthorId);

        var newSlug = ContentSlugParser.ParseContentSlug(slug);
        var contentEntity = new ContentEntity
        {
            Id = 1,
            ShopId = newSlug.ShopId,
            SectionType = newSlug.SectionType,
            IsVisible = updateModel.IsVisible,
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = staff.FullName,
        };

        MockStaffRepository(staff);

        MockContentRepository(null);

        _contentRepositoryMock
            .AddAsync(contentEntity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(contentEntity));

        //Act
        var request = new UpdateContentBySlugCommand(slug, AuthorId, updateModel);
        await _handler.Handle(request, CancellationToken.None);

        //Assert
        await _staffRepositoryMock
            .Received(1)
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>());

        await _contentRepositoryMock
            .Received(1)
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>());

        await _contentRepositoryMock
            .Received(1)
            .AddAsync(Arg.Is<ContentEntity>(x => x.ShopId == newSlug.ShopId && x.SectionType == newSlug.SectionType), Arg.Any<CancellationToken>());

        await _contentRepositoryMock
            .Received(0)
            .UpdateAsync(Arg.Any<ContentEntity>(), Arg.Any<CancellationToken>());
    }
    private void MockStaffRepository(StaffEntity? staff)
    {
        _staffRepositoryMock
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(staff));
    }
    private void MockContentRepository(ContentEntity? contentBlock)
    {
        _contentRepositoryMock
            .GetAsync(Arg.Any<GetContentByShopAndSectionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(contentBlock));
    }
    private StaffEntity CreateStaff(Guid id)
    {
        return new StaffEntity
        {
            Id = id,
            FirstName = "something",
            LastName = "goodLastName",
            Email = "test@gmail.com",
            PasswordHash = "test-password-hash"
        };
    }
    private UpdateContentRequest CreateUpdateModel(string content = "<p>Updated content</p>", bool isVisible = true)
    {
        return new UpdateContentRequest
        {
            Content = content,
            IsVisible = isVisible
        };
    }
}
