using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OfferSection
{
    public class GetVisiblePromotionOffersSectionSpecification
        : Specification<OfferEntity>
    {
        public GetVisiblePromotionOffersSectionSpecification()
            : base(s => s.IsVisible == true)
        {
            AddInclude(s => s.Products);                      // join table links
            AddOrderBy(s => s.LastUpdated, descending: true); // latest definition
        }
    }
}