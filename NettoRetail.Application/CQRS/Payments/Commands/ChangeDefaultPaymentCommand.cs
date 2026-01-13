using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record ChangeDefaultPaymentCommand(string PaymentMethodId, Guid AccountId) : IRequest<bool>;

public class ChangeDefaultPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeDefaultPaymentCommand, bool>
{
    public async Task<bool> Handle(ChangeDefaultPaymentCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentMethodsByAccountSpecification(request.PaymentMethodId, request.AccountId);

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

        var newDefaultStripeId = paymentMethod.StripePaymentMethodId;

        await stripePaymentService.SetDefaultPaymentMethodAsync(
            customerId,
            newDefaultStripeId,
            cancellationToken);

        using var tx = unitOfWork.BeginTransaction();
        try
        {
            foreach (var pm in allMethods)
                pm.IsDefault = pm.StripePaymentMethodId == newDefaultStripeId;

            await paymentRepository.UpdateManyAsync(allMethods, cancellationToken);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
