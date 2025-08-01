using Mapster;
using MediatR;
using NettoRetail.Application.Models.NewProductsSection;
using NettoRetail.DataAccess.Specifications.NewProductSection;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.NewProductsSection.Queries;

public record GetNewProductsSectionByShopIdQuery(int ShopId) : IRequest<GetNewProductsSectionResponse?>;

public class GetNewProductsSectionByShopIdQueryHandler(
    INewProductsRepository repository)
    : IRequestHandler<GetNewProductsSectionByShopIdQuery, GetNewProductsSectionResponse?>
{
  public async Task<GetNewProductsSectionResponse?> Handle(
      GetNewProductsSectionByShopIdQuery request,
      CancellationToken cancellationToken)
  {
    var spec = new GetNewProductsSectionByShopIdSpecification(request.ShopId);
    var entity = await repository.GetAsync(spec, cancellationToken);

    if (entity is null) return null;

    return entity.Adapt<GetNewProductsSectionResponse>();
  }
}
