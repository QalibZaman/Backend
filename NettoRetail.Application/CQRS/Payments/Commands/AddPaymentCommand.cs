using Mapster;
using MediatR;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Payments;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record AddPaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest;

public sealed class AddPaymentCommandHandler(
    IAccountRepository accountRepository,
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService
) : IRequestHandler<AddPaymentCommand>
{
    public async Task Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.AccountId == Guid.Empty)
            throw new ArgumentException("AccountId is empty.", nameof(request.AccountId));

        if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
            throw new ArgumentException("PaymentMethodId is required.", nameof(request.PaymentMethodId));

        var account = await accountRepository.GetAsync(
            new GetAccountByIdSpecification(request.AccountId),
            cancellationToken);

        if (account is null)
            throw new KeyNotFoundException($"Account '{request.AccountId}' not found.");

        if (await paymentRepository.ExistsByStripePaymentMethodIdAsync(request.PaymentMethodId, cancellationToken))
            throw new ArgumentException("Payment method already exists.");

        var fullName = $"{account.FirstName} {account.LastName}".Trim();

        var customerId = await stripePaymentService.EnsureCustomerAsync(
            account.StripeCustomerId,
            fullName,
            account.Email,
            cancellationToken);

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
    }
}
