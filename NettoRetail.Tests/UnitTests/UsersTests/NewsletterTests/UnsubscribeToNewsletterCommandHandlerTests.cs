using FluentAssertions;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.NewsletterTests;

public class UnsubscribeToNewsletterCommandHandlerTests
{
    private readonly UnsubscribeToNewsletterCommandHandler _handler;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailBuilderService _emailBuilderService;

    // Note: the handler constructor takes ILogger<SubscribeToNewsletterCommandHandler>,
    // not ILogger<UnsubscribeToNewsletterCommandHandler> - matches the current source as-is.
    private readonly ILogger<SubscribeToNewsletterCommandHandler> _logger;

    public UnsubscribeToNewsletterCommandHandlerTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _logger = Substitute.For<ILogger<SubscribeToNewsletterCommandHandler>>();

        _handler = new UnsubscribeToNewsletterCommandHandler(
            _staffRepository,
            _emailSender,
            _emailBuilderService,
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var command = new UnsubscribeToNewsletterCommand(Guid.NewGuid());

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((StaffEntity?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Account not found");

        await _emailBuilderService
            .DidNotReceive()
            .BuildNewsletterUnsubscribedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenNotSubscribed()
    {
        // Arrange
        var command = new UnsubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: false);

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("You are not subscribed to newsletter");

        await _emailBuilderService
            .DidNotReceive()
            .BuildNewsletterUnsubscribedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUnsubscribeAndSendEmail_WhenAccountIsEligible()
    {
        // Arrange
        var command = new UnsubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: true);
        var emailModel = new EmailModel { Email = account.Email };

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _emailBuilderService
            .BuildNewsletterUnsubscribedEmailAsync(account.Email, account.FullName, Arg.Any<int>())
            .Returns(emailModel);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        account.IsSubscribedToNewsletter.Should().BeFalse();

        await _emailSender
            .Received(1)
            .SendAsync(emailModel, command.UserId);
        await _staffRepository
            .Received(1)
            .UpdateAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldBuildEmailWithCurrentYear_WhenUnsubscribing()
    {
        // Arrange
        var command = new UnsubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: true);
        var emailModel = new EmailModel { Email = account.Email };
        var expectedYear = DateTime.UtcNow.Year;

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _emailBuilderService
            .BuildNewsletterUnsubscribedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(emailModel);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailBuilderService
            .Received(1)
            .BuildNewsletterUnsubscribedEmailAsync(account.Email, account.FullName, expectedYear);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_AndNotUpdateAccount_WhenEmailSendingFails()
    {
        // Arrange
        var command = new UnsubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: true);
        var emailModel = new EmailModel { Email = account.Email };

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _emailBuilderService
            .BuildNewsletterUnsubscribedEmailAsync(account.Email, account.FullName, Arg.Any<int>())
            .Returns(emailModel);
        _emailSender
            .SendAsync(Arg.Any<EmailModel>(), Arg.Any<Guid>())
            .Returns<Task>(_ => throw new InvalidOperationException("SMTP is unavailable"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();

        account.IsSubscribedToNewsletter.Should().BeTrue();
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    private static StaffEntity CreateStaffEntity(bool isSubscribed)
    {
        return new StaffEntity
        {
            FirstName = "Test",
            LastName = "Testtest",
            Email = "test@gmail.com",
            IsSubscribedToNewsletter = isSubscribed,
        };
    }
}