using System.ComponentModel.DataAnnotations;
using System.Data;
using FluentAssertions;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.PaymentTests.Delete;

public class DeletePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStripeService _stripeService;
    private readonly IDbTransaction _transaction;
    private readonly DeletePaymentCommandHandler _handler;

    public DeletePaymentCommandHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _stripePaymentService = Substitute.For<IStripePaymentService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _stripeService = Substitute.For<IStripeService>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new DeletePaymentCommandHandler(
            _paymentRepository,
            _stripePaymentService,
            _unitOfWork,
            _stripeService);
    }

    [Fact]
    public async Task Handle_ShouldDeletePayment_AndReturnNotLast_WhenDeletingNonDefaultPayment()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateAccount("cus_123");
        var paymentToDelete = CreatePayment(accountId, "pm_delete", account, isDefault: false);
        var currentDefault = CreatePayment(accountId, "pm_default", account, isDefault: true);
        account.PaymentMethods = new List<PaymentMethodEntity> { currentDefault, paymentToDelete };

        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentToDelete);
        _stripeService
            .PaymentMethodExistsAsync(paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(new DeletePaymentCommand(accountId, paymentToDelete.StripePaymentMethodId), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new PaymentMethodDeleteResponse { IsLastPaymentMethod = false });

        await _stripePaymentService.DidNotReceive()
            .SetDefaultPaymentMethodAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _paymentRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());
        await _stripePaymentService.Received(1)
            .DetachPaymentMethodAsync(paymentToDelete.StripePaymentMethodId);
        await _paymentRepository.Received(1)
            .DeleteAsync(paymentToDelete, Arg.Any<CancellationToken>());

        _unitOfWork.Received(1).BeginTransaction();
        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldSetFallbackAsDefault_WhenDeletingDefaultPaymentAndFallbackExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateAccount("cus_123");
        var paymentToDelete = CreatePayment(accountId, "pm_default", account, isDefault: true);
        var fallback = CreatePayment(accountId, "pm_fallback", account, isDefault: false);
        account.PaymentMethods = new List<PaymentMethodEntity> { paymentToDelete, fallback };

        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentToDelete);
        _stripeService
            .PaymentMethodExistsAsync(paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(true);
        _paymentRepository
            .GetFallbackPaymentMethodAsync(accountId, paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(fallback);

        // Act
        var result = await _handler.Handle(new DeletePaymentCommand(accountId, paymentToDelete.StripePaymentMethodId), CancellationToken.None);

        // Assert
        result.IsLastPaymentMethod.Should().BeFalse();

        await _stripePaymentService.Received(1)
            .SetDefaultPaymentMethodAsync(account.StripeCustomerId!, fallback.StripePaymentMethodId, Arg.Any<CancellationToken>());
        await _paymentRepository.Received(1)
            .UpdateAsync(Arg.Is<PaymentMethodEntity>(p => p.StripePaymentMethodId == fallback.StripePaymentMethodId && p.IsDefault), Arg.Any<CancellationToken>());
        await _stripePaymentService.Received(1)
            .DetachPaymentMethodAsync(paymentToDelete.StripePaymentMethodId);
        await _paymentRepository.Received(1)
            .DeleteAsync(paymentToDelete, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnLastMethodTrue_WhenDeletingOnlyPaymentMethod()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateAccount("cus_123");
        var paymentToDelete = CreatePayment(accountId, "pm_only", account, isDefault: true);
        account.PaymentMethods = new List<PaymentMethodEntity> { paymentToDelete };

        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentToDelete);
        _stripeService
            .PaymentMethodExistsAsync(paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(true);
        _paymentRepository
            .GetFallbackPaymentMethodAsync(accountId, paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns((PaymentMethodEntity?)null);

        // Act
        var result = await _handler.Handle(new DeletePaymentCommand(accountId, paymentToDelete.StripePaymentMethodId), CancellationToken.None);

        // Assert
        result.IsLastPaymentMethod.Should().BeTrue();

        await _stripePaymentService.DidNotReceive()
            .SetDefaultPaymentMethodAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _paymentRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());
        await _stripePaymentService.Received(1)
            .DetachPaymentMethodAsync(paymentToDelete.StripePaymentMethodId);
        await _paymentRepository.Received(1)
            .DeleteAsync(paymentToDelete, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPaymentDoesNotExistInDatabase()
    {
        // Arrange
        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((PaymentMethodEntity?)null);

        // Act
        var act = async () => await _handler.Handle(new DeletePaymentCommand(Guid.NewGuid(), "pm_missing"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

        await _stripeService.DidNotReceive()
            .PaymentMethodExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPaymentDoesNotExistInStripe()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateAccount("cus_123");
        var paymentToDelete = CreatePayment(accountId, "pm_missing_in_stripe", account, isDefault: false);
        account.PaymentMethods = new List<PaymentMethodEntity> { paymentToDelete };

        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentToDelete);
        _stripeService
            .PaymentMethodExistsAsync(paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(new DeletePaymentCommand(accountId, paymentToDelete.StripePaymentMethodId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

        await _stripePaymentService.DidNotReceive()
            .DetachPaymentMethodAsync(Arg.Any<string>());
        await _paymentRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDeletingDefaultAndStripeCustomerIsMissing()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateAccount(string.Empty);
        var paymentToDelete = CreatePayment(accountId, "pm_default", account, isDefault: true);
        var fallback = CreatePayment(accountId, "pm_fallback", account, isDefault: false);
        account.PaymentMethods = new List<PaymentMethodEntity> { paymentToDelete, fallback };

        _paymentRepository
            .GetAsync(Arg.Any<GetPaymentMethodByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentToDelete);
        _stripeService
            .PaymentMethodExistsAsync(paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(true);
        _paymentRepository
            .GetFallbackPaymentMethodAsync(accountId, paymentToDelete.StripePaymentMethodId, Arg.Any<CancellationToken>())
            .Returns(fallback);

        // Act
        var act = async () => await _handler.Handle(new DeletePaymentCommand(accountId, paymentToDelete.StripePaymentMethodId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(HandlerConstants.Stripe.StripeCustomerIsNotConfiguredMessage);

        await _stripePaymentService.DidNotReceive()
            .DetachPaymentMethodAsync(Arg.Any<string>());
        await _paymentRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());
        await _paymentRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<PaymentMethodEntity>(), Arg.Any<CancellationToken>());
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    private static BaseUserEntity CreateAccount(string? stripeCustomerId)
    {
        return new BaseUserEntity
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hash",
            StripeCustomerId = stripeCustomerId,
            PaymentMethods = new List<PaymentMethodEntity>()
        };
    }

    private static PaymentMethodEntity CreatePayment(
        Guid accountId,
        string stripePaymentMethodId,
        BaseUserEntity account,
        bool isDefault)
    {
        return new PaymentMethodEntity
        {
            AccountId = accountId,
            Account = account,
            StripePaymentMethodId = stripePaymentMethodId,
            FourLastCardNumbers = "4242",
            CardBrand = "Visa",
            CardExpMonth = 12,
            CardExpYear = DateTime.UtcNow.Year + 1,
            IsDefault = isDefault,
            IsActive = true
        };
    }
}
