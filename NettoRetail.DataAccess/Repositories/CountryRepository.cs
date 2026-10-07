using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class CountryRepository(AppDbContext dbContext)
        : BaseRepository<CountryEntity>(dbContext),
          ICountryRepository
    {
    }
}
