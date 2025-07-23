using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.NewProductSection;

public class GetProductsBySectionIdSpecification : Specification<NewProductsProductEntity>
{
  public GetProductsBySectionIdSpecification(int sectionId)
      : base(x => x.SectionId == sectionId)
  {
  }
}
