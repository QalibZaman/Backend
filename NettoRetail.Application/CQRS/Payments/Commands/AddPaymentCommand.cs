using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record AddPaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest<Unit>;

public sealed class AddPaymentCommandHandler(
    IStaffRepository accountRepository,
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<AddPaymentCommand, Unit>
{
    public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(request.AccountId),
            cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Payment.AccountNotFoundMessage, request.AccountId));

        if (await paymentRepository.ExistsByStripePaymentMethodIdAsync(request.PaymentMethodId, cancellationToken))
            throw new BadRequestException(HandlerConstants.Payment.PaymentMethodAlreadyExistsMessage);

        var customerId = await stripePaymentService.EnsureCustomerAsync(
            account.StripeCustomerId,
            account.FullName,
            account.Email,
            cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        if (!string.Equals(account.StripeCustomerId, customerId, StringComparison.Ordinal))
        {
            account.StripeCustomerId = customerId;
            await accountRepository.UpdateAsync(account, cancellationToken);
        }

        var card = await stripePaymentService.AttachAndGetCardAsync(
            request.PaymentMethodId,
            customerId,
            cancellationToken);

        var paymentMethodRecord = new PaymentMethodEntity
        {
            AccountId = account.Id,
            StripePaymentMethodId = request.PaymentMethodId,
            CardBrand = card.Brand,
            CardExpMonth = card.ExpMonth,
            CardExpYear = card.ExpYear,
            FourLastCardNumbers = card.Last4,
            CreatedAt = DateTime.UtcNow
        };

        await paymentRepository.AddAsync(paymentMethodRecord, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }
}
