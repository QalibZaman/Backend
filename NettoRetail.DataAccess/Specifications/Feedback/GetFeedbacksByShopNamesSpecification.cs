using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByShopNamesSpecification : Specification<FeedbackEntity>
{
  public GetFeedbacksByShopNamesSpecification(string shopName)
      : base(o => o.ShopName == shopName)
{
}
}