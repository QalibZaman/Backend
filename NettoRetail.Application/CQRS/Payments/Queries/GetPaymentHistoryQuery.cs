using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Queries;

public record GetPaymentHistoryQuery(PaginationRequest PageModel, Guid UserId)
    : IRequest<PaginatedResponse<PaymentHistoryResponse>>;

public class GetPaymentHistoryQueryHandler(
    ChargeService chargeService,
    IAccountRepository accountRepository,
    IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<GetPaymentHistoryQuery, PaginatedResponse<PaymentHistoryResponse>>
{
    public async Task<PaginatedResponse<PaymentHistoryResponse>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.UserId);
        var account = await accountRepository.GetAsync(specification, cancellationToken);

        if (string.IsNullOrEmpty(account?.StripeCustomerId))
        {
            return new PaginatedResponse<PaymentHistoryResponse>
            {
                Items = [],
                TotalAmount = 0
            };
        }

        var options = new ChargeListOptions
        {
            Customer = account.StripeCustomerId,
            Limit = request.PageModel.PageSize,
        };

        var charges = await chargeService.ListAsync(options, cancellationToken: cancellationToken);

        var stripeTransactionIds = charges.Data.Select(c => c.Id).ToList();
        var existingTransactionIds = await paymentHistoryRepository
            .GetExistingTransactionIdsAsync(stripeTransactionIds, cancellationToken);

        var newEntities = charges.Data
            .Where(c => !existingTransactionIds.Contains(c.Id))
            .Select(charge => new PaymentHistoryEntity
            {
                AccountId = account.Id,
                PaymentDate = charge.Created,
                CustomerName = charge.BillingDetails?.Name ?? "N/A",
                AmountCharged = charge.Amount / 100m,
                PaymentMethod = charge.PaymentMethodDetails?.Type ?? "N/A",
                PlanName = charge.Metadata.TryGetValue("planName", out var plan) ? plan : "N/A",
                BillingPeriod = charge.Metadata.TryGetValue("billingPeriod", out var period) ? period : "N/A",
                Status = charge.Status,
                TransactionId = charge.Id,
                Account = account
            })
            .ToList();

        if (newEntities.Any())
        {
            await paymentHistoryRepository.AddManyAsync(newEntities, cancellationToken);
        }

        var responseItems = charges.Data.Select(charge => new PaymentHistoryResponse
        {
            PaymentDate = charge.Created,
            CustomerName = charge.BillingDetails?.Name ?? "N/A",
            AmountCharged = charge.Amount / 100m,
            PaymentMethod = charge.PaymentMethodDetails?.Type ?? "N/A",
            PlanName = charge.Metadata.TryGetValue("planName", out var plan) ? plan : "N/A",
            BillingPeriod = charge.Metadata.TryGetValue("billingPeriod", out var period) ? period : "N/A",
            Status = charge.Status,
            TransactionId = charge.Id
        }).ToList();

        return new PaginatedResponse<PaymentHistoryResponse>
        {
            Items = responseItems,
            TotalAmount = charges.Data.Count
        };
    }
}
