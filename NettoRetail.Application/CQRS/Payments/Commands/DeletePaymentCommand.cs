using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record DeletePaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest<PaymentMethodDeleteResponse>;

public class DeletePaymentCommandHandler(IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork,
    IStripeService stripeService) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    public async Task<PaymentMethodDeleteResponse> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId);
        var payment = await paymentRepository.GetAsync(specification, cancellationToken)
                            ?? throw new NotFoundException("Payment method not found in DB");

        if (!await stripeService.PaymentMethodExistsAsync(request.PaymentMethodId, cancellationToken))
        {
            throw new NotFoundException("Payment method not found in Stripe");
        }

        if (payment.IsDefault)
            await ChangeDefaultPaymentMethod(request.AccountId, request.PaymentMethodId, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();
        await stripePaymentService.DetachPaymentMethodAsync(request.PaymentMethodId);
        await paymentRepository.DeleteAsync(payment, cancellationToken);
        transaction.Commit();

        var isLastMethod = payment.Account.PaymentMethods?.Count == 1;

        var response = new PaymentMethodDeleteResponse
        {
            IsLastPaymentMethod = isLastMethod
        };

        return response;
    }

    private async Task ChangeDefaultPaymentMethod(Guid accountId, string excludedPaymentMethodId, CancellationToken cancellationToken)
    {
        var fallbackMethod = await paymentRepository.GetFallbackPaymentMethodAsync(accountId, excludedPaymentMethodId, cancellationToken);

        if (fallbackMethod != null)
        {
            fallbackMethod.IsDefault = true;
            await paymentRepository.UpdateAsync(fallbackMethod, cancellationToken);
        }
    }
}