using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByShopNamesSpecification(string shopName) : Specification<FeedbackEntity>(o => o.ShopName == shopName)
{
}