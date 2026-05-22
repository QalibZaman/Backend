using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductReview
{
    public class GetProductReviewByIdSpecification : Specification<ProductReviewEntity>
    {
        public GetProductReviewByIdSpecification(Guid id)
            : base(ProductReview => ProductReview.Id == id)
        {
            AddInclude(ProductReview => ProductReview.Customer);
        }
    }
}
