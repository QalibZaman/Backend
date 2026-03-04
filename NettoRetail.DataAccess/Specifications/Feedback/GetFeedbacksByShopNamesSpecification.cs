using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByShopNamesSpecification : Specification<FeedbackEntity>
{
  public GetFeedbacksByShopNamesSpecification(string shopName)
      : base(o => EF.Functions.ILike(o.ShopName, $"%{shopName.Trim()}%"))
{
}
}
