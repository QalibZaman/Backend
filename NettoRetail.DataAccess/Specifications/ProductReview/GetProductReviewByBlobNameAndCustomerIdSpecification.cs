using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductReview;

public class GetProductReviewByBlobNameAndCustomerIdSpecification : Specification<ProductReviewEntity>
{
    public GetProductReviewByBlobNameAndCustomerIdSpecification(string blobName, Guid customerId)
        : base(x => x.CustomerId == customerId && x.PhotoUrls != null && EF.Functions.JsonContains(x.PhotoUrls, $"\"{blobName}\""))
    {
    }
}
