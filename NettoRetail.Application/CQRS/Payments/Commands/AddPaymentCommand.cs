using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record AddPaymentCommand(Guid AccountId, int ShopId, string PaymentMethodId) : IRequest<Unit>;

public sealed class AddPaymentCommandHandler(
    IStaffRepository accountRepository,
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IShopRepository shopRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddPaymentCommand, Unit>
{
    public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var account = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(request.AccountId),
            cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Account.AccountNotFoundMessage, request.AccountId));

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

        if (!string.Equals(shop.StripeCustomerId, customerId, StringComparison.Ordinal))
        {
            shop.StripeCustomerId = customerId;
            await shopRepository.UpdateAsync(shop, cancellationToken);
        }

        var card = await stripePaymentService.AttachAndGetCardAsync(
            request.PaymentMethodId,
            customerId,
            cancellationToken);

        var paymentMethodRecord = new PaymentMethodEntity
        {
            AccountId = account.Id,
            StripePaymentMethodId = card.StripePaymentMethodId,
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
