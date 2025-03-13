using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Country;

public class GetCountryByIdSpecification : Specification<CountryEntity>
{
    public GetCountryByIdSpecification(int id)
   : base(x => x.Id == id) { }
}
