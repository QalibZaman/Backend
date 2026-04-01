using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Offer;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.OfferSection;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.OfferSections.Command;

public record UpdateOffersSectionCommand(
    UpdateOfferSectionRequest Model,
    int ShopId,
    Guid UserId) : IRequest;

public class UpdateOffersSectionCommandHandler(
    IOfferRepository offerRepo,
    IOffersProductRepository productRepo,
    IInventoryItemRepository inventoryItemRepository,
    IStaffRepository accountRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateOffersSectionCommand>
{
    public async Task Handle(UpdateOffersSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOfferSectionByShopIdSpecification(request.ShopId);
        var offer = await offerRepo.GetAsync(spec, cancellationToken);

        await ValidateItemIds(request.Model.InventoryItemIds, request.ShopId, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        if (offer is null)
            offer = MapToNewOffer(request);
        else
        {
            UpdateOffer(offer, request.Model);

            var deleteSpec = new GetProductsByOfferSectionIdSpecification(offer.Id);
            await productRepo.DeleteManyBySpecAsync(deleteSpec, cancellationToken);
        }

        offer.UpdatedBy = await GetUpdatedBy(request.UserId, cancellationToken);

        offer.Products = BuildLinkedProducts(request.Model.InventoryItemIds, offer);

        if (offer is null)
            await offerRepo.AddAsync(offer, cancellationToken);
        else
            await offerRepo.UpdateAsync(offer, cancellationToken);

        transaction.Commit();
    }

    private static OfferEntity MapToNewOffer(UpdateOffersSectionCommand request) => new()
    {
        ShopId = request.ShopId,
        Name = request.Model.Name,
        IsVisible = request.Model.IsVisible,
        SelectionType = request.Model.SelectionType,
        LastUpdated = DateTime.UtcNow,
    };

    private static void UpdateOffer(OfferEntity offer, UpdateOfferSectionRequest model)
    {
        offer.Name = model.Name.Trim();
        offer.IsVisible = model.IsVisible;
        offer.SelectionType = model.SelectionType;
        offer.LastUpdated = DateTime.UtcNow;
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

    private async Task<string> GetUpdatedBy(Guid userId, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(userId);
        var user = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("User not found");

        return user.FullName;
    }

    private async Task ValidateItemIds(List<Guid> itemIds, int shopId, CancellationToken cancellationToken)
    {
        if (itemIds.Count != itemIds.Distinct().Count())
        {
            throw new BadRequestException("Duplicate inventory items are not allowed.");
        }

        foreach (var item in itemIds)
        {
            if (!await inventoryItemRepository.Exists(new GetInventoryItemByIdAndShopIdSpecification(item, shopId), cancellationToken))
                throw new NotFoundException("This item is not in your shop.");
        }
    }
}