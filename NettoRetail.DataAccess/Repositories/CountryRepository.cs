using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class CountryRepository : BaseRepository<CountryEntity>, ICountryRepository
{
    public CountryRepository(AppDbContext dbContext)
        : base(dbContext)
    { }

    public CountryEntity Get(Specification<CountryEntity> specification)
    {
        return _dbSet.Where(specification.Criteria).First();
    }
}
