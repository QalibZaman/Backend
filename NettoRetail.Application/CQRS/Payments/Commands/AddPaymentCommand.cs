using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record AddPaymentCommand(Guid AccountId, int ShopId, string PaymentMethodId) : IRequest<Unit>;

public sealed class AddPaymentCommandHandler(
    IStaffRepository staffRepository,
    IShopRepository shopRepository,
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<AddPaymentCommand, Unit>
{
    public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Payment.ShopNotFoundMessage, request.ShopId));

        var account = await staffRepository.GetAsync(new GetStaffByIdSpecification(request.AccountId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Payment.AccountNotFoundMessage, request.AccountId));

        if (account.ShopId != request.ShopId)
        {
            throw new ForbiddenException(HandlerConstants.Payment.CannotAddPaymentMethodForAnotherShopMessage);
        }

        var isFirstPaymentMethod = !await paymentRepository.HasAnyByAccountIdAsync(request.AccountId, cancellationToken);

        var customerId = await stripePaymentService.EnsureCustomerAsync(
            account.StripeCustomerId,
            $"{account.FirstName} {account.LastName}",
            account.Email,
            cancellationToken);

        var card = await stripePaymentService.AttachAndGetCardAsync(
            request.PaymentMethodId,
            customerId,
            cancellationToken);

        var isDuplicate = await paymentRepository.ExistsDuplicateCardAsync(
            request.AccountId,
            card.Brand,
            card.Last4,
            card.ExpMonth,
            card.ExpYear,
            cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException(HandlerConstants.Payment.PaymentMethodAlreadyExistsMessage);
        }

        using var transaction = unitOfWork.BeginTransaction();

        if (!string.Equals(account.StripeCustomerId, customerId, StringComparison.Ordinal))
        {
            account.StripeCustomerId = customerId;
            await staffRepository.UpdateAsync(account, cancellationToken);
        }

        if (!string.Equals(shop.StripeCustomerId, customerId, StringComparison.Ordinal))
        {
            shop.StripeCustomerId = customerId;
            await shopRepository.UpdateAsync(shop, cancellationToken);
        }

        if (isFirstPaymentMethod)
        {
            await stripePaymentService.SetDefaultPaymentMethodAsync(customerId, card.StripePaymentMethodId, cancellationToken);
        }

        var paymentMethodRecord = new PaymentMethodEntity
        {
            AccountId = account.Id,
            StripePaymentMethodId = card.StripePaymentMethodId,
            CardBrand = card.Brand,
            CardExpMonth = card.ExpMonth,
            CardExpYear = card.ExpYear,
            FourLastCardNumbers = card.Last4,
            IsActive = true,
            IsDefault = isFirstPaymentMethod,
            CreatedAt = DateTime.UtcNow
        };

        await paymentRepository.AddAsync(paymentMethodRecord, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }
}