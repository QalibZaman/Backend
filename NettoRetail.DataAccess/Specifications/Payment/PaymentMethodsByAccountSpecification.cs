using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NettoRetail.DataAccess.Specifications.Payment;

public class PaymentMethodsByAccountSpecification : Specification<PaymentMethodEntity>
{
    public PaymentMethodsByAccountSpecification(string id, Guid accountId)
        : base(p => p.StripePaymentMethodId == id && p.AccountId == accountId)
    {
        AddInclude(p => p.Account.PaymentMethods);
    }
}