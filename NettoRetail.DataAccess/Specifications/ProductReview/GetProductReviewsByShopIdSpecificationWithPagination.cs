using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductReview;

public class GetProductReviewsByShopIdSpecificationWithPagination : Specification<ProductReviewEntity>
{
    public GetProductReviewsByShopIdSpecificationWithPagination(Guid customerId, int skip, int take)
        : base(x => x.CustomerId == customerId)
    {
        Skip = skip;
        Take = take;
    }
}
