using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.DataAccess.Specifications.ProductReview
{
    public class HasCustomerReviewedProductSpecification : Specification<ProductReviewEntity>
    {
        public HasCustomerReviewedProductSpecification(Guid customerId, Guid inventoryItemId)
            : base(ProductReview => ProductReview.CustomerId == customerId && ProductReview.InventoryItemId == inventoryItemId) 
        {

        }
    }
}
