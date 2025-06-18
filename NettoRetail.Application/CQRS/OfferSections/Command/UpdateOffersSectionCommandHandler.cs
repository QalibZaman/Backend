using Mapster;
using MediatR;
using NettoRetail.DataAccess.Specifications.OfferSection;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.OfferSections.Command;

public record UpdateOffersSectionCommand(
    int ShopId,
    string Name,
    string? Preview,
    bool IsVisible,
    SelectionTypeEnum SelectionType,
    List<Guid> InventoryItemIds,
    string UpdatedBy
) : IRequest;

public class UpdateOffersSectionCommandHandler(
    IOfferRepository offerRepo,
    IOffersProductRepository productRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateOffersSectionCommand>
{
    public async Task Handle(UpdateOffersSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOfferSectionByShopIdSpecification(request.ShopId);
        var offer = await offerRepo.GetAsync(spec, cancellationToken);

        var transaction = unitOfWork.BeginTransaction();

        if (offer is null)
        {
            offer = MapToNewOffer(request);
            offer.Products = BuildLinkedProducts(request.InventoryItemIds, offer);

            await offerRepo.AddAsync(offer, cancellationToken);
        }
        else
        {
            UpdateOffer(offer, request);

            var deleteSpec = new GetProductsByOfferSectionIdSpecification(offer.Id);
            await productRepo.DeleteManyBySpecAsync(deleteSpec, cancellationToken);

            offer.Products = BuildLinkedProducts(request.InventoryItemIds, offer);
            await offerRepo.UpdateAsync(offer, cancellationToken);
        }

        transaction.Commit();
    }

    private static OfferEntity MapToNewOffer(UpdateOffersSectionCommand request) => new()
    {
        ShopId = request.ShopId,
        Name = request.Name.Trim(),
        Preview = request.Preview?.Trim(),
        IsVisible = request.IsVisible,
        SelectionType = request.SelectionType,
        LastUpdated = DateTime.UtcNow,
        UpdatedBy = request.UpdatedBy
    };

    private static void UpdateOffer(OfferEntity offer, UpdateOffersSectionCommand request)
    {
        offer.Name = request.Name.Trim();
        offer.Preview = request.Preview?.Trim();
        offer.IsVisible = request.IsVisible;
        offer.SelectionType = request.SelectionType;
        offer.LastUpdated = DateTime.UtcNow;
        offer.UpdatedBy = request.UpdatedBy;
    }

    private static List<OffersProductEntity> BuildLinkedProducts(
        List<Guid> inventoryItemIds,
        OfferEntity offer)
    {
        return inventoryItemIds.Select((itemId, index) => new OffersProductEntity
        {
            InventoryItemId = itemId,
            DisplayOrder = index,
            Offer = offer,
            OfferId = offer.Id
        }).ToList();
    }
}