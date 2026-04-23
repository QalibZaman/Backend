using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record DeletePaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest<PaymentMethodDeleteResponse>;

public class DeletePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork,
    IStripeService stripeService) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    public async Task<PaymentMethodDeleteResponse> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await GetPaymentMethodAsync(request, cancellationToken);

        if (!await stripeService.PaymentMethodExistsAsync(request.PaymentMethodId, cancellationToken))
        {
            throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);
        }

        var isLastMethod = payment.Account.PaymentMethods?.Count == 1;
        var fallbackMethod = await GetFallbackMethodIfNeededAsync(request, payment, cancellationToken);

        if (fallbackMethod != null)
        {
            await SetStripeFallbackAsDefaultAsync(payment, fallbackMethod, cancellationToken);
        }

        using var transaction = unitOfWork.BeginTransaction();

        if (fallbackMethod != null)
        {
            fallbackMethod.IsDefault = true;
            await paymentRepository.UpdateAsync(fallbackMethod, cancellationToken);
        }

        await stripePaymentService.DetachPaymentMethodAsync(request.PaymentMethodId);
        await paymentRepository.DeleteAsync(payment, cancellationToken);

        transaction.Commit();

        return new PaymentMethodDeleteResponse
        {
            IsLastPaymentMethod = isLastMethod
        };
    }

    private async Task<PaymentMethodEntity> GetPaymentMethodAsync(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId);

        return await paymentRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);
    }

    private async Task<PaymentMethodEntity?> GetFallbackMethodIfNeededAsync(
        DeletePaymentCommand request,
        PaymentMethodEntity payment,
        CancellationToken cancellationToken)
    {
        if (!payment.IsDefault)
        {
            return null;
        }

        return await paymentRepository.GetFallbackPaymentMethodAsync(
            request.AccountId,
            request.PaymentMethodId,
            cancellationToken);
    }

    private async Task SetStripeFallbackAsDefaultAsync(
        PaymentMethodEntity paymentToDelete,
        PaymentMethodEntity fallbackMethod,
        CancellationToken cancellationToken)
    {
        var customerId = paymentToDelete.Account.StripeCustomerId;

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ValidationException(HandlerConstants.Stripe.StripeCustomerIsNotConfiguredMessage);
        }

        await stripePaymentService.SetDefaultPaymentMethodAsync(
            customerId,
            fallbackMethod.StripePaymentMethodId,
            cancellationToken);
    }
}
