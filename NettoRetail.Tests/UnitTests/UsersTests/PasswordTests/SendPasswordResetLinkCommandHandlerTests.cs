using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Tests.UnitTests.UsersTests.PasswordTests;

public class SendPasswordResetLinkCommandHandlerTests
{
    private readonly IStaffRepository _accountRepo;
    private readonly IEmailBuilderService _emailBuilder;
    private readonly IEmailSender _emailSender;
    private readonly SendPasswordResetLinkCommandHandler _handler;

    public SendPasswordResetLinkCommandHandlerTests()
    {
        _accountRepo = Substitute.For<IStaffRepository>();
        _emailBuilder = Substitute.For<IEmailBuilderService>();
        _emailSender = Substitute.For<IEmailSender>();

        _handler = new SendPasswordResetLinkCommandHandler(
            _accountRepo,
            _emailBuilder,
            _emailSender);
    }

    [Fact]
    public async Task Handle_ShouldSendResetPasswordEmail_WhenAccountExists()
    {
        var email = "test.user@example.com";
        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = PasswordHasher.HashPassword("DummyP@ssword1")
        };

        _accountRepo.GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);

        var emailModel = new EmailModel { Email = email, Subject = "Reset", Message = "link" };
        _emailBuilder.BuildResetPasswordEmailAsync(email, staff.FirstName)
            .Returns(emailModel);

        await _handler.Handle(new SendPasswordResetLinkCommand(email), CancellationToken.None);

        await _emailBuilder.Received(1).BuildResetPasswordEmailAsync(email, staff.FirstName);
        await _emailSender.Received(1).SendAsync(emailModel, staff.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAccountIsMissing()
    {
        _accountRepo.GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((StaffEntity?)null);

        await Assert.ThrowsAsync<BadRequestException>(async () =>
            await _handler.Handle(new SendPasswordResetLinkCommand("missing@example.com"), CancellationToken.None));
    }
}
