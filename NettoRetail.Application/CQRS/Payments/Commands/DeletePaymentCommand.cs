using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record DeletePaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest<PaymentMethodDeleteResponse>;

public class DeletePaymentCommandHandler(IPaymentRepository paymentRepository,
    PaymentMethodService paymentMethodService,
    IUnitOfWork unitOfWork,
    ILogger<DeletePaymentCommandHandler> logger) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    public async Task<PaymentMethodDeleteResponse> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId);
        var payment = await paymentRepository.GetAsync(specification, cancellationToken)
                            ?? throw new NotFoundException("Payment method not found in DB");
        var paymentMethod = await paymentMethodService.GetAsync(request.PaymentMethodId)
                            ?? throw new NotFoundException("Payment method not found in Stripe");

        if (payment.IsDefault)
            await ChangeDefaultPaymentMethod(request.AccountId, request.PaymentMethodId, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();
        try
        {
            await paymentRepository.DeleteAsync(payment, cancellationToken);
            await paymentMethodService.DetachAsync(request.PaymentMethodId);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Failed to delete payment");
            throw new PaymentExceptions(ex.Message);
        }

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