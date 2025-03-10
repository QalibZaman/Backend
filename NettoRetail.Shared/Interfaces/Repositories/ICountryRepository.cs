using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICountryRepository : IBaseRepository<CountryEntity>
{
    CountryEntity Get(Specification<CountryEntity> specification);
}
