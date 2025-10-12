using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Comparison;

public class ComparisonListByUserAndCategorySpec : Specification<ComparisonListEntity>
{
    public ComparisonListByUserAndCategorySpec(Guid customerId, int categoryId, bool includeItems = true)
        : base(comparisonListEntity => comparisonListEntity.CustomerId == customerId && comparisonListEntity.CategoryId == categoryId)
    {
        if (includeItems)
        {
            AddInclude(comparisonListEntity => comparisonListEntity.ComparisonListItems);
        }
    }
}