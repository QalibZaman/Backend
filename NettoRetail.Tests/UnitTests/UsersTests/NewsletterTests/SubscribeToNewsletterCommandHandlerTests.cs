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

public class SubscribeToNewsletterCommandHandlerTests
{
    private readonly SubscribeToNewsletterCommandHandler _handler;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SubscribeToNewsletterCommandHandler> _logger;

    public SubscribeToNewsletterCommandHandlerTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Substitute.For<ILogger<SubscribeToNewsletterCommandHandler>>();

        _handler = new SubscribeToNewsletterCommandHandler(
            _staffRepository,
            _emailBuilderService,
            _emailSender,
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(Guid.NewGuid());

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
            .BuildNewsletterSubscribedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>());
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAlreadySubscribed()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: true);

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("You are already subscribed to newsletters");

        await _emailBuilderService
            .DidNotReceive()
            .BuildNewsletterSubscribedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>());
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSubscribeAndSendEmail_WhenAccountIsEligible()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: false);
        var emailModel = new EmailModel { Email = account.Email };

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _emailBuilderService
            .BuildNewsletterSubscribedEmailAsync(account.Email, account.FullName, command.UserId)
            .Returns(emailModel);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        account.IsSubscribedToNewsletter.Should().BeTrue();

        await _emailSender
            .Received(1)
            .SendAsync(emailModel, command.UserId);
        await _staffRepository
            .Received(1)
            .UpdateAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_AndNotUpdateAccount_WhenEmailSendingFails()
    {
        // Arrange
        var command = new SubscribeToNewsletterCommand(Guid.NewGuid());
        var account = CreateStaffEntity(isSubscribed: false);
        var emailModel = new EmailModel { Email = account.Email };

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _emailBuilderService
            .BuildNewsletterSubscribedEmailAsync(account.Email, account.FullName, command.UserId)
            .Returns(emailModel);
        _emailSender
            .SendAsync(Arg.Any<EmailModel>(), Arg.Any<Guid>())
            .Returns<Task>(_ => throw new InvalidOperationException("SMTP is unavailable"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();

        account.IsSubscribedToNewsletter.Should().BeFalse();
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