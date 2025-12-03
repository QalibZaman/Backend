using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.OfferSection;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OfferSections.Queries;

public record GetPromotionOffersQuery() : IRequest<IEnumerable<InventoryItemCardResponse>>;

public class GetPromotionOffersQueryHandler(
    IOfferRepository offerRepository)
    : IRequestHandler<GetPromotionOffersQuery, IEnumerable<InventoryItemCardResponse>>
{
    public async Task<IEnumerable<InventoryItemCardResponse>> Handle(GetPromotionOffersQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetVisiblePromotionOffersSectionSpecification();
        var offers = await offerRepository.GetManyBySpecAsync(specification, cancellationToken);

        var inventoryItems = offers.SelectMany(x => x.Products.Select(n => n.InventoryItem)).ToList();
        var response = inventoryItems.Adapt<List<InventoryItemCardResponse>>();
        return response;
    }
}
