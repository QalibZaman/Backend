using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IProductDetailRepository : IBaseRepository<ProductDetailEntity>
{
    Task UpdateDetailsAsync(Specification<ProductDetailEntity> upsertSpecification, Specification<ProductDetailEntity> deleteSpecification, CancellationToken cancellationToken = default);
} 