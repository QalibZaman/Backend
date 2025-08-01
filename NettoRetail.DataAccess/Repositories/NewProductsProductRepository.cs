using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class NewProductsProductRepository(AppDbContext dbContext) : BaseRepository<NewProductsProductEntity>(dbContext), INewProductsProductRepository
{
}
