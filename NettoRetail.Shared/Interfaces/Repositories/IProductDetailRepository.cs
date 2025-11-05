using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IProductDetailRepository : IBaseRepository<ProductDetailEntity>
{
    Task UpdateDetailsAsync(List<Guid> updatedDetailsIds, ICollection<ProductDetailEntity> newDetails, Specification<ProductDetailEntity> deleteSpecification, CancellationToken cancellationToken = default);
} 