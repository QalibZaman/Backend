using AngleSharp.Dom;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Data;

namespace NettoRetail.Tests.UnitTests.PaymentTests.AddPayment;

public class AddPaymentCommandHandlerTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AddPaymentCommandHandler _handler;
    private readonly IDbTransaction _transaction;
    public AddPaymentCommandHandlerTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _stripePaymentService = Substitute.For<IStripePaymentService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new AddPaymentCommandHandler(_staffRepository, _paymentRepository, _stripePaymentService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldAddPaymentMethod_WhenValidRequest()
    {
        // Arrange
        var staffEntity = InitializeEntity();

        var command = new AddPaymentCommand(staffEntity.Id, "pm_card_mastercard");
        var newCustomerId = "cus_12345";
        var cardInfo = new CardSnapshot("Visa", 12, 2025, "4242");

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffEntity);

        _paymentRepository
            .ExistsByStripePaymentMethodIdAsync(command.PaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(false);

        _stripePaymentService
            .EnsureCustomerAsync(null, staffEntity.FullName, staffEntity.Email, Arg.Any<CancellationToken>())
            .Returns(newCustomerId);

        _stripePaymentService
            .AttachAndGetCardAsync(command.PaymentMethodId, newCustomerId, Arg.Any<CancellationToken>())
            .Returns(cardInfo);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _staffRepository
            .Received(1)
            .UpdateAsync(Arg.Is<StaffEntity>(s => s.Id == staffEntity.Id && s.StripeCustomerId == newCustomerId), Arg.Any<CancellationToken>());

        await _paymentRepository
            .Received(1)
            .AddAsync(Arg.Is<PaymentMethodEntity>(p =>
            p.CardBrand == cardInfo.Brand &&
            p.FourLastCardNumbers == cardInfo.Last4 &&
            p.StripePaymentMethodId == command.PaymentMethodId), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
        _transaction.Received(0).Rollback();


    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAccountDoesNotExist()
    {
        //Arrange
        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((StaffEntity?)null));

        var command = new AddPaymentCommand(Guid.NewGuid(), "pm_123");

        //Act 
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(string.Format(HandlerConstants.Payment.AccountNotFoundMessage, command.AccountId));

        _unitOfWork.DidNotReceive().BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenPaymentMethodAlreadyExists()
    {
        //Arrange
        var account = InitializeEntity();

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);

        _paymentRepository
            .ExistsByStripePaymentMethodIdAsync("pm_already_exists", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new AddPaymentCommand(account.Id, "pm_already_exists");

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(HandlerConstants.Payment.PaymentMethodAlreadyExistsMessage);

        _unitOfWork.DidNotReceive().BeginTransaction();
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateAccount_WhenStripeCustomerIdAlreadyExists()
    {
        //Arrange
        var existingId = "existingId";
        var staffEntity = InitializeEntity();
        staffEntity.StripeCustomerId = existingId;
        var command = new AddPaymentCommand(staffEntity.Id, "pm_card_mastercard");
        var cardInfo = new CardSnapshot("Marsercard", 12, 2025, "4242");

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffEntity);

        _stripePaymentService
            .EnsureCustomerAsync(existingId, staffEntity.FullName,staffEntity.Email, Arg.Any<CancellationToken>())
            .Returns(existingId);

        _stripePaymentService
            .AttachAndGetCardAsync(command.PaymentMethodId, existingId, Arg.Any<CancellationToken>())
            .Returns(cardInfo);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _staffRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());

        await _paymentRepository
            .Received(1)
            .AddAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldNotSavePayment_WhenStripeServiceThrowsException()
    {
        //Arrange
        var staffEntity = InitializeEntity();

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffEntity);

        _stripePaymentService
            .EnsureCustomerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),  Arg.Any<CancellationToken>())
            .Returns("newCustomerId");

        _stripePaymentService
            .AttachAndGetCardAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new PaymentExceptions("Error"));

        //Act
        var command = new AddPaymentCommand(staffEntity.Id, "pm_invalid");
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<PaymentExceptions>()
            .WithMessage("Error");

        await _paymentRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.DidNotReceive().Commit();

    }
    private StaffEntity InitializeEntity()
    {
        return new StaffEntity
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testtest",
            PasswordHash = "hashedpassword",
        };
    }
}
