using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.OfferSection;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OfferSections.Queries;

public record GetPromotionOffersQuery(int ShopId) : IRequest<IEnumerable<InventoryItemCardResponse>>;

public class GetPromotionOffersQueryHandler(
    IOfferRepository offerRepository)
    : IRequestHandler<GetPromotionOffersQuery, IEnumerable<InventoryItemCardResponse>>
{
    public async Task<IEnumerable<InventoryItemCardResponse>> Handle(GetPromotionOffersQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetVisiblePromotionOffersSectionSpecification(request.ShopId);
        var offers = await offerRepository.GetManyBySpecAsync(specification, cancellationToken);
        if (offers is null || !offers.Any())
        {
            throw new NotFoundException("There are no offers.");
        }

        var inventoryItems = offers.Select(x => x.Products.Select(n => n.InventoryItem)).ToList();
        var response = inventoryItems.Adapt<List<InventoryItemCardResponse>>();
        return response;
    }
}
