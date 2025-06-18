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
    List<int> ProductIds,
    string UpdatedBy) : IRequest;

public class UpdateOffersSectionCommandHandler(
    IOfferRepository sectionRepo,
    IOffersProductRepository productRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateOffersSectionCommand>
{
    public async Task Handle(UpdateOffersSectionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOfferSectionByShopIdSpecification(request.ShopId);
        var existingSection = await sectionRepo.GetAsync(spec, cancellationToken);

        var transaction = unitOfWork.BeginTransaction();

        if (existingSection is null)
        {
            var newSection = request.Adapt<OfferEntity>();
            newSection.Products = CreateOfferProducts(request.ProductIds, newSection);

            await sectionRepo.AddAsync(newSection, cancellationToken);
        }
        else
        {
            UpdateSection(existingSection, request);

            var deleteSpec = new GetProductsByOfferSectionIdSpecification(existingSection.Id);
            await productRepo.DeleteManyBySpecAsync(deleteSpec, cancellationToken);

            existingSection.Products = CreateOfferProducts(request.ProductIds, existingSection);
            await sectionRepo.UpdateAsync(existingSection, cancellationToken);
        }

        transaction.Commit();
    }

    private static void UpdateSection(OfferEntity section, UpdateOffersSectionCommand request)
    {
        section.Name = request.Name.Trim();
        section.Preview = request.Preview?.Trim();
        section.IsVisible = request.IsVisible;
        section.SelectionType = request.SelectionType;
        section.LastUpdated = DateTime.UtcNow;
        section.UpdatedBy = request.UpdatedBy;
    }

    private static List<OffersProductEntity> CreateOfferProducts(
        List<int> productIds,
        OfferEntity parentSection)
    {
        return productIds.Select((pid, index) => new OffersProductEntity
        {
            ProductId = pid,
            DisplayOrder = index,
            Offer = parentSection,
            OfferId = parentSection.Id
        }).ToList();
    }
}