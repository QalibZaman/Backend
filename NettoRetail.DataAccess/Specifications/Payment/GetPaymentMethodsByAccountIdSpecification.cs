using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment;

public class GetPaymentMethodsByAccountIdSpecification : Specification<PaymentMethodEntity>
{
    public GetPaymentMethodsByAccountIdSpecification( Guid accountId)
        : base(p => p.AccountId == accountId)
    {
        AddInclude(p => p.Account.PaymentMethods);
    }
}