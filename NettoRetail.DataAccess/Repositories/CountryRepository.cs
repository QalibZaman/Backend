using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class CountryRepository : BaseRepository<CountryEntity>, ICountryRepository
{
    public CountryRepository(AppDbContext dbContext)
        : base(dbContext)
    { }
}
