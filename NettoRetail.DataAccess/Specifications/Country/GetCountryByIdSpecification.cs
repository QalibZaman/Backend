using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Country
{
    public sealed class GetCountryByIdSpecification
      : Specification<CountryEntity>
    {
        public GetCountryByIdSpecification(int countryId)
            : base(country => country.Id == countryId)
        {
        }
    }
}
