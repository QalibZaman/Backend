using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record ChangeDefaultPaymentCommand(string PaymentMethodId, Guid AccountId) : IRequest<bool>;

public class ChangeDefaultPaymentCommandHandler(IPaymentRepository paymentRepository, PaymentMethodService paymentMethodService)
    : IRequestHandler<ChangeDefaultPaymentCommand, bool>
{
    public async Task<bool> Handle(ChangeDefaultPaymentCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentMethodsByAccountSpecification(request.PaymentMethodId, request.AccountId);
        var paymentMethod = await paymentRepository.GetAsync(spec, cancellationToken)
                            ?? throw new NotFoundException("Payment method not found.");

        var allMethods = paymentMethod.Account.PaymentMethods?.ToList() ?? throw new NotFoundException("No payment methods found for the account.");

        allMethods.ForEach(pm => pm.IsDefault = (pm.StripePaymentMethodId == request.PaymentMethodId));

        foreach (var method in allMethods)
        {
            await paymentRepository.UpdateAsync(method, cancellationToken);
        }

        await paymentMethodService.UpdateAsync(paymentMethod.StripePaymentMethodId, null, null, cancellationToken);

        return true;
    }
}