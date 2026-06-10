using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Payment;

public class GetPaymentHistoriesByUserIdSpecification : Specification<PaymentHistoryEntity>
{
    public GetPaymentHistoriesByUserIdSpecification(Guid userId, bool sortDesc, int skip, int take)
        : base(x => userId == Guid.Empty || x.AccountId == userId)
    {
        AddInclude(x => x.Account);
        AddInclude(x => x.Account.Shop);
        AddOrderBy(x => x.PaymentDate, sortDesc);
        Skip = skip;
        Take = take;
    }
}
