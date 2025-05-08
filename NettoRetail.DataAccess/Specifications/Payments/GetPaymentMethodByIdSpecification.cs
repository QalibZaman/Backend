using Azure.Storage.Blobs.Models;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payments;

public class GetPaymentMethodByIdSpecification : Specification<PaymentMethodEntity>
{
    public GetPaymentMethodByIdSpecification(Guid accountId, string paymentMethodId)
        : base(o => 
            o.AccountId == accountId &&
            o.StripePaymentMethodId == paymentMethodId)
    {
        AddInclude(x => x.Account);
    }
}
