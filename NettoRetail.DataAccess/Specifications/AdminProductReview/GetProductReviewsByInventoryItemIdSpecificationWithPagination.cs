using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.AdminProductReview;

public class GetProductReviewsByInventoryItemIdSpecificationWithPagination : Specification<ProductReviewEntity>
{
    public GetProductReviewsByInventoryItemIdSpecificationWithPagination(Guid inventoryItemId, int skip, int take)
        : base(x => x.InventoryItemId == inventoryItemId)
    {
        Skip = skip;
        Take = take;
        AddOrderByDescending(x => x.DateSubmitted);
    }
}