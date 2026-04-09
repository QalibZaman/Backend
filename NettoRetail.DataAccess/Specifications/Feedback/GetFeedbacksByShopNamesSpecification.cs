using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Feedback;

public class GetFeedbacksByShopNamesSpecification : Specification<FeedbackEntity>
{
    public GetFeedbacksByShopNamesSpecification(string shopName)
        : base(Specification<FeedbackEntity>.Empty)
    {
        var normalized = shopName.Trim();
        AddCriteria(f => EF.Functions.ILike(f.ShopName, $"%{normalized}%"));
    }
}
