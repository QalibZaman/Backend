using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Payments;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record EditPaymentCommand(Guid AccountId, EditPaymentRequest Request) : IRequest;

public sealed class EditPaymentCommandHandler(
    IStaffRepository staffRepository,
    IPaymentRepository paymentRepository,
    PaymentMethodService paymentMethodService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<EditPaymentCommand>
{
    public async Task Handle(EditPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.AccountId == Guid.Empty)
            throw new ArgumentException(string.Format(HandlerConstants.Payment.AccountIsEmptyMessage, request.AccountId));

        var account = await staffRepository.GetAsync(
            new GetStaffByIdSpecification(request.AccountId),
            cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Payment.AccountNotFoundMessage, request.AccountId));

        var spec = new GetPaymentMethodByIdSpecification(account.Id, request.Request.StripePaymentMethodId);

        var existing = await paymentRepository.GetAsync(spec, cancellationToken);

        if (existing == null)
            throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

        var stripePaymentMethod = await paymentMethodService.GetAsync(
            request.Request.StripePaymentMethodId,
            cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(stripePaymentMethod.CustomerId))
        {
            stripePaymentMethod = await paymentMethodService.AttachAsync(
                request.Request.StripePaymentMethodId,
                new PaymentMethodAttachOptions
                {
                    Customer = account.StripeCustomerId
                },
                cancellationToken: cancellationToken);
        }

        stripePaymentMethod = await paymentMethodService.UpdateAsync(
            request.Request.StripePaymentMethodId,
            new PaymentMethodUpdateOptions
            {
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Name = request.Request.CardHolderName
                }
            },
            cancellationToken: cancellationToken);

        var activeSpec = new ActivePaymentMethodsByUserIdSpecification(account.Id);
        var activeMethods = await paymentRepository.GetAllBySpecAsync(activeSpec, cancellationToken);
        using var transaction = unitOfWork.BeginTransaction();

        foreach (var method in activeMethods)
           {
            if (method.Id == existing.Id)
                continue;

            method.IsActive = false;
            await paymentRepository.UpdateAsync(method, cancellationToken);
           }

        existing.CardExpMonth = stripePaymentMethod.Card?.ExpMonth ?? 0;
        existing.CardExpYear = stripePaymentMethod.Card?.ExpYear ?? 0;
        existing.FourLastCardNumbers = stripePaymentMethod.Card?.Last4 ?? string.Empty;
        existing.IsActive = true;

        await paymentRepository.UpdateAsync(existing, cancellationToken);
        transaction.Commit();
    }
}