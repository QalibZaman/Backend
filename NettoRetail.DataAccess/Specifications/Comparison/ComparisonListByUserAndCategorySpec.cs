using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Comparison
{
    public class ComparisonListByUserAndCategorySpec : Specification<ComparisonListEntity>
    {
        public ComparisonListByUserAndCategorySpec(string userId, int categoryId, bool includeItems = true)
            : base(comparisonListEntity => comparisonListEntity.UserId == userId && comparisonListEntity.CategoryId == categoryId)
        {
            if (includeItems)
            {
                AddInclude(comparisonListEntity => comparisonListEntity.ComparisonListItems);
            }
        }
    }
}
