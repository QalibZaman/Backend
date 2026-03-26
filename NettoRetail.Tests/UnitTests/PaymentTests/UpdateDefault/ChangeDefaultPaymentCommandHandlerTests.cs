using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace NettoRetail.Tests.UnitTests.PaymentTests.UpdateDefault;

public class ChangeDefaultPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ChangeDefaultPaymentCommandHandler _handler;

    public ChangeDefaultPaymentCommandHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _stripePaymentService = Substitute.For<IStripePaymentService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ChangeDefaultPaymentCommandHandler(_paymentRepository, _stripePaymentService, _unitOfWork);
    }

    [Fact]
    public async Task Handler_ShouldChangeDefaultPayment_WhenRequestIsValid()
    {
        //Arrange
        var command = SetupValidCommand();

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _paymentRepository
            .Received(1).GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>());

        await _paymentRepository
            .Received(1).UpdateManyAsync(Arg.Any<List<PaymentMethodEntity>>(), Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
    }

    [Fact]
    public async Task Handler_ThrowsNotFoundException_WhenPaymentMethodNotFound()
    {
        //Arrange
        var command = SetupValidCommand();

        _paymentRepository
            .GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns((PaymentMethodEntity?)null);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

    }

    [Fact]
    public async Task Handler_ThrowsValidationException_WhenOnlyOnePaymentMethodExists()
    {
        //Arrange
        var accountId = Guid.NewGuid();
        var command = new ChangeDefaultPaymentCommand("pm_23223233223232323", accountId);

        var account = GenerateAccount();

        var paymentMethod = GeneratePaymentMethodEntity(accountId, "pm_23223233223232323", account);
        account.PaymentMethods!.Add(paymentMethod);

        _paymentRepository
            .GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentMethod);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(HandlerConstants.Payment.AtLeastTwoPaymentMethodsRequiredMessage);
    }

    [Fact]
    public async Task Handler_ThrowsValidationException_WhenCustomerIsNotConfigured()
    {
        //Arrange
        var accountId = Guid.NewGuid();
        var command = new ChangeDefaultPaymentCommand("pm_232323223323232", accountId);

        var account = GenerateAccount();
        account.StripeCustomerId = string.Empty;

        var first = GeneratePaymentMethodEntity(accountId, "pm_232323223323232", account);
        var second = GeneratePaymentMethodEntity(accountId, "pm_23233232544554", account);

        account.PaymentMethods!.Add(first);
        account.PaymentMethods!.Add(second);

        _paymentRepository
            .GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(first);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(HandlerConstants.Stripe.StripeCustomerIsNotConfiguredMessage);
    }

    [Fact]
    public async Task Handler_ThrowsValidationException_WhenPaymentMethodDoesntBelongToAccount()
    {
        //Arrange
        var command = SetupValidCommand();
        var paymentMethod = GeneratePaymentMethodEntity(Guid.NewGuid(), "pm_323323323", GenerateAccount());

        _paymentRepository
            .GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentMethod);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(HandlerConstants.Payment.PaymentMethodDoesntBelongMessage);
    }

    private ChangeDefaultPaymentCommand SetupValidCommand()
    {
        var accountId = Guid.NewGuid();
        var stripePaymentMethodId = "pm_23223233223232323";
        var stripePaymentMethodIdSecond = "pm_23233232544554";
        var account = GenerateAccount();

        var command = new ChangeDefaultPaymentCommand(stripePaymentMethodId, accountId);

        var first = GeneratePaymentMethodEntity(accountId, stripePaymentMethodId, account);
        var second = GeneratePaymentMethodEntity(accountId, stripePaymentMethodIdSecond, account);

        account.PaymentMethods!.Add(first);
        account.PaymentMethods!.Add(second);

        _paymentRepository
            .GetAsync(Arg.Any<PaymentMethodsByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(first);

        return command;
    }
    private BaseUserEntity GenerateAccount()
    {

        var account = new BaseUserEntity
        {
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Test",
            PasswordHash = "hash",
            StripeCustomerId = "cus_test123",
            PaymentMethods = new List<PaymentMethodEntity>()
        };
        return account;
    }
    private PaymentMethodEntity GeneratePaymentMethodEntity(Guid accountId, string stripePaymentMethodId, BaseUserEntity account)
    {
        return new PaymentMethodEntity
        {
            AccountId = accountId,
            Account = account,
            StripePaymentMethodId = stripePaymentMethodId,
        };
    }
}
