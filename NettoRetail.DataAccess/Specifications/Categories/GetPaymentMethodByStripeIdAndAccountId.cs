using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetPaymentMethodByStripeIdAndAccountId(
    string stripePaymentMethodId,
    Guid accountId)
    : Specification<PaymentMethodEntity>(
        payment => payment.StripePaymentMethodId == stripePaymentMethodId &&
                   payment.AccountId == accountId)
{
}