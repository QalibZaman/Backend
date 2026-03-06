using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.NewProductsSection;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.NewProductSection;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.NewProductsSection.Commands;

public record UpdateNewProductsSectionCommand(
    int ShopId,
    UpdateNewProductsSectionRequest Model,
    string UpdatedBy) : IRequest<Unit>;

public class UpdateNewProductsSectionCommandHandler(
    INewProductsRepository sectionRepo,
    INewProductsProductRepository productRepo,
    IInventoryService inventoryService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateNewProductsSectionCommand, Unit>
{
    public async Task<Unit> Handle(UpdateNewProductsSectionCommand request, CancellationToken cancellationToken)
    {
        var areAllProductsBelongToShop = await inventoryService.AreAllItemsBelongToShopAsync(
            request.ShopId, request.Model.InventoryItemIds, cancellationToken);
        if (!areAllProductsBelongToShop) throw new BadRequestException("All products must be related to your store");

        var specification = new GetNewProductsSectionByShopIdSpecification(request.ShopId);
        var section = await sectionRepo.GetAsync(specification, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        if (section is null)
        {
            section = MapToNewSection(request);
        }
        else
        {
            UpdateSection(section, request);

            var deleteSpec = new GetProductsBySectionIdSpecification(section.Id);
            await productRepo.DeleteManyBySpecAsync(deleteSpec, cancellationToken);
        }

        section.Products = BuildLinkedProducts(request.Model.InventoryItemIds, section);

        if (section is { Id: 0 })
            await sectionRepo.AddAsync(section, cancellationToken);
        else
            await sectionRepo.UpdateAsync(section, cancellationToken);

        transaction.Commit();

        return Unit.Value;

        //The current version of the handler works for both POST and PUT.
        //This handler requires refactoring after the BA clarifies the requirements.
    }

    private static NewProductsSectionEntity MapToNewSection(UpdateNewProductsSectionCommand request) => new()
    {
        ShopId = request.ShopId,
        Name = request.Model.Name.Trim(),
        IsVisible = request.Model.IsVisible,
        SelectionType = request.Model.SelectionType,
        LastUpdated = DateTime.UtcNow,
        UpdatedBy = request.UpdatedBy
    };

    private static void UpdateSection(NewProductsSectionEntity section, UpdateNewProductsSectionCommand request)
    {
        section.Name = request.Model.Name.Trim();
        section.IsVisible = request.Model.IsVisible;
        section.SelectionType = request.Model.SelectionType;
        section.LastUpdated = DateTime.UtcNow;
        section.UpdatedBy = request.UpdatedBy;
    }
    private static List<NewProductsProductEntity> BuildLinkedProducts(List<Guid> ids, NewProductsSectionEntity section)
    {
        return ids.Select((id, i) => new NewProductsProductEntity
        {
            InventoryItemId = id,
            DisplayOrder = i,
            Section = section,
            SectionId = section.Id
        }).ToList();
    }
}
