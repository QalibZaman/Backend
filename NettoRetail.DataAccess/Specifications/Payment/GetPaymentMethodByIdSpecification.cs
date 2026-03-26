using Azure.Storage.Blobs.Models;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment;

public class GetPaymentMethodByIdSpecification : Specification<PaymentMethodEntity>
{
    public GetPaymentMethodByIdSpecification(Guid accountId, string stripePaymentMethodId)
        : base(o =>
            o.AccountId == accountId &&
            o.StripePaymentMethodId == stripePaymentMethodId)
    {
        AddInclude(x => x.Account);
        AddInclude(x => x.Account.PaymentMethods);
    }
}
