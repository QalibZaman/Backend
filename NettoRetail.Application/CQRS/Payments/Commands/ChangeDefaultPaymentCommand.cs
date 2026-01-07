using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record ChangeDefaultPaymentCommand(string PaymentMethodId, Guid AccountId) : IRequest<bool>;

public class ChangeDefaultPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService
) : IRequestHandler<ChangeDefaultPaymentCommand, bool>
{
    public async Task<bool> Handle(ChangeDefaultPaymentCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentMethodsByAccountSpecification(
            request.PaymentMethodId,
            request.AccountId);

        var paymentMethod = await paymentRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Payment method not found.");

        var allMethods = paymentMethod.Account.PaymentMethods?.ToList()
            ?? throw new NotFoundException("No payment methods found for the account.");

        if (allMethods.Count < 2)
            throw new ValidationException("At least two payment methods are required to change default.");

        if (paymentMethod.IsDefault)
            return true;

        var customerId = paymentMethod.Account.StripeCustomerId;
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ValidationException("Stripe customer is not configured for this account.");

        try
        {
            await stripePaymentService.SetDefaultPaymentMethodAsync(
                customerId,
                paymentMethod.StripePaymentMethodId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to update default payment method.", ex);
        }

        foreach (var pm in allMethods)
            pm.IsDefault = pm.StripePaymentMethodId == request.PaymentMethodId;

        try
        {
            await paymentRepository.UpdateManyAsync(allMethods, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Failed to save default payment method.", ex);
        }

        return true;
    }
}
