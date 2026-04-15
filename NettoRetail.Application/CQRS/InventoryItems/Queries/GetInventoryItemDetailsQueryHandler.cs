using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemDetailsQuery(Guid Id) : IRequest<IEnumerable<ProductDetailsResponse>>;
public class GetInventoryItemDetailsQueryHandler(
    IProductDetailRepository productDetailRepository,
    IMapper mapper)
    : IRequestHandler<GetInventoryItemDetailsQuery, IEnumerable<ProductDetailsResponse>>
{
    public async Task<IEnumerable<ProductDetailsResponse>> Handle(GetInventoryItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var detailsSpecification = new GetProductDetailsByItemIdSpecification(request.Id);

        var productDetails = await productDetailRepository.GetAllBySpecAsync(detailsSpecification, cancellationToken);

        var response = mapper.Map<IEnumerable<ProductDetailsResponse>>(productDetails);

        return response;
    }
}
