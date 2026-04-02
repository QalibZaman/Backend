using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PasswordHistory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Tests.UnitTests.UsersTests.PasswordTests;

public class ResetPasswordCommandHandlerTests
{
    private readonly IStaffRepository _accountRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly ISessionTerminationService _sessionTerminationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtManager _jwtManager;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _accountRepository = Substitute.For<IStaffRepository>();
        _passwordHistoryRepository = Substitute.For<IPasswordHistoryRepository>();
        _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _jwtManager = Substitute.For<IJwtManager>();

        _handler = new ResetPasswordCommandHandler(
            _accountRepository,
            _passwordHistoryRepository,
            _sessionTerminationService,
            _unitOfWork,
            _jwtManager);
    }

    [Fact]
    public async Task Handle_ShouldValidateResetTokenAndUpdateStaffStatus_WhenTokenIsValid()
    {
        // Arrange
        var token = "reset-token";
        var email = "user@example.com";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, email)
        }));

        _jwtManager.ValidateResetToken(token).Returns(claimsPrincipal);

        var currentPassword = "OldP@ssword1";
        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(currentPassword),
            Status = StaffStatusEnum.CreatePassword
        };

        _accountRepository.GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);

        _passwordHistoryRepository.GetAllBySpecAsync(Arg.Any<GetPasswordHistoryByAccountIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<PasswordHistoryEntity>());

        _passwordHistoryRepository.AddAsync(Arg.Any<PasswordHistoryEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<PasswordHistoryEntity>(null!));

        _accountRepository.UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(staff));

        _sessionTerminationService.TerminateSessionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var transaction = Substitute.For<IDbTransaction>();
        _unitOfWork.BeginTransaction().Returns(transaction);

        var request = new ResetPasswordCommand(new ResetPasswordRequest(token, "NewP@ssword1", "NewP@ssword1"));

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _jwtManager.Received(1).ValidateResetToken(token);
        _jwtManager.DidNotReceive().Validate(token);
        _passwordHistoryRepository.Received(1).AddAsync(Arg.Any<PasswordHistoryEntity>(), Arg.Any<CancellationToken>());
        await _accountRepository.Received(1).UpdateAsync(Arg.Is<StaffEntity>(u => u.Status == StaffStatusEnum.Active && u.PasswordUpdatedAt != default), Arg.Any<CancellationToken>());
        transaction.Received(1).Commit();
        await _sessionTerminationService.Received(1).TerminateSessionsAsync(staff.Id, Arg.Any<CancellationToken>());
    }
}
