using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment;

public class GetPaymentMethodByIdSpecification : Specification<PaymentMethodEntity>
{
    public GetPaymentMethodByIdSpecification(Guid accountId, string stripePaymentMethodId)
        : base(p => p.AccountId == accountId && p.StripePaymentMethodId == stripePaymentMethodId)
    {
        AddInclude(p => p.Account);
    }
}