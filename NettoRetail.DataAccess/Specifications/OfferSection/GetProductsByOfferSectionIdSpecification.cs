using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OfferSection
{
    public class GetProductsByOfferSectionIdSpecification : Specification<OffersProductEntity>
    {
        public GetProductsByOfferSectionIdSpecification(int sectionId)
            : base(x => x.OfferId == sectionId)
        {
        }
    }
}
