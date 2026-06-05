using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductReview;

public class GetProductReviewByIdAndShopIdSpecification : Specification<ProductReviewEntity>
{
    public GetProductReviewByIdAndShopIdSpecification(Guid id, Guid customerId)
        : base (x => x.Id == id && x.CustomerId == customerId)
    {
    }
}
