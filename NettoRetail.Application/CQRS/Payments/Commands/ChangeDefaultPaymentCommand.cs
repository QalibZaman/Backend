using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record ChangeDefaultPaymentCommand(string StripePaymentMethodId, Guid AccountId) : IRequest;

public class ChangeDefaultPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeDefaultPaymentCommand>
{
    public async Task Handle(ChangeDefaultPaymentCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentMethodsByAccountSpecification(request.StripePaymentMethodId, request.AccountId);

        var paymentMethod = await paymentRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

        if (paymentMethod.AccountId != request.AccountId)
            throw new ValidationException(HandlerConstants.Payment.PaymentMethodDoesntBelongMessage);

        if (paymentMethod.CardExpYear < DateTime.UtcNow.Year || (paymentMethod.CardExpYear == DateTime.UtcNow.Year && paymentMethod.CardExpMonth < DateTime.UtcNow.Month))
            throw new ValidationException(HandlerConstants.Payment.PaymentMethodExpiredMessage);

        var allMethods = paymentMethod.Account.PaymentMethods!.ToList();

        if (allMethods.Count < 2)
            throw new ValidationException(HandlerConstants.Payment.AtLeastTwoPaymentMethodsRequiredMessage);

        if (paymentMethod.IsDefault)
            return;

        var customerId = paymentMethod.Account.StripeCustomerId;
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ValidationException(HandlerConstants.Stripe.StripeCustomerIsNotConfiguredMessage);

        var newDefaultStripeId = paymentMethod.StripePaymentMethodId;

        using var transaction = unitOfWork.BeginTransaction();

        foreach (var payMethod in allMethods)
                payMethod.IsDefault = payMethod.StripePaymentMethodId == newDefaultStripeId;

        await paymentRepository.UpdateManyAsync(allMethods, cancellationToken);

        await stripePaymentService.SetDefaultPaymentMethodAsync(
            customerId,
            newDefaultStripeId,
            cancellationToken);

        transaction.Commit();
    }
}
