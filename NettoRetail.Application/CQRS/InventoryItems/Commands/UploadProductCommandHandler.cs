using Mapster;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateProductCommand(Guid Id, int ShopId, UpdateProductModel Model) : IRequest;

public class UpdateProductCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IProductDetailRepository productDetailRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productId = request.Id;
        var shopId = request.ShopId;

        var model = request.Model;

        var productSpec = new GetInventoryItemByIdAndShopIdSpecification(productId, shopId);

        var item = await inventoryItemRepository.GetAsync(productSpec, cancellationToken) ??
                   throw new NotFoundException("The product does not exist.");

        var transaction = unitOfWork.BeginTransaction();

        if (model.GeneralInfo is not null)
        {
            await ValidateGeneralInfoAsync(model.GeneralInfo, shopId, cancellationToken);

            mapper.Map(model.GeneralInfo, item);
        }

        if (model.PriceInfo is not null)
        {
            ValidatePrice(model.PriceInfo, item);

            mapper.Map(model.PriceInfo, item);
        }

        if (model.AvailabilityInfo is not null)
        {
            model.AvailabilityInfo.Adapt(item);
        }

        if (model.Details is not null)
        {
            var detailsForModification = await TryFetchDetailsAsync(model.Details, cancellationToken);

            await PerformDeletionOfNecessaryDetails(model.Details, detailsForModification, cancellationToken);

            await PerformUpdateOfNecessaryDetails(model.Details, detailsForModification, cancellationToken);

            await PerformAdditionOfNecessaryDetails(model.Details, item, cancellationToken);
        }

        item.LastUpdatedDate = DateTime.UtcNow;

        await inventoryItemRepository.UpdateAsync(item, cancellationToken);

        transaction.Commit();
    }

    private async Task PerformDeletionOfNecessaryDetails(IList<DetailModel> models, IList<ProductDetailEntity> entities,
        CancellationToken cancellationToken)
    {
        var detailIdsForDeletion = models.Where((m) => m.IsDelete)
            .Select((m) => m.Id)
            .ToList();

        if (detailIdsForDeletion.Any())
        {
            var entitiesForDeletion = entities.IntersectBy(detailIdsForDeletion, (e) => e.Id);

            await productDetailRepository.DeleteRangeAsync(entitiesForDeletion, cancellationToken);
        }
    }

    private async Task PerformUpdateOfNecessaryDetails(IList<DetailModel> models, IList<ProductDetailEntity> entities,
        CancellationToken cancellationToken)
    {
        var modelsToUpdate = models.Where(m => !m.IsDelete).ToList();

        if (modelsToUpdate.Any())
        {
            var modelIds = modelsToUpdate
                .Select(m => m.Id)
                .ToHashSet();
            var entitiesToUpdate = entities
                .Where(e => modelIds.Contains(e.Id))
                .ToList();
            foreach (var entity in entitiesToUpdate)
            {
                var sourceModel = modelsToUpdate.First(m => m.Id == entity.Id);

                mapper.Map(sourceModel, entity);
            }

            await productDetailRepository.UpdateDetailsAsync(entitiesToUpdate, cancellationToken);
        }
    }

    private async Task PerformAdditionOfNecessaryDetails(IList<DetailModel> details, InventoryItemEntity item,
        CancellationToken cancellationToken)
    {
        int order = 0;
        var detailsToAdd = details
            .Where((d) => !d.IsDelete && !d.Id.HasValue)
            .Select((detail) =>
            {
                return new ProductDetailEntity()
                {
                    Name = detail.Name!,
                    TypeEnum = detail.TypeEnum!.Value,
                    Unit = detail.TypeEnum!.Value == ProductDetailTypeEnum.Number ? detail.Unit!.Value : null,
                    Value = detail.Value!,
                    InventoryItemId = item.Id,
                    OrderIndex = detail.OrderIndex.HasValue ? detail.OrderIndex.Value : order++
                };
            })
            .ToList();

        if (detailsToAdd.Any())
        {
            await productDetailRepository.AddRangeAsync(detailsToAdd, cancellationToken);
        }
    }

    private async Task<IList<ProductDetailEntity>> TryFetchDetailsAsync(IList<DetailModel> details,
        CancellationToken cancellationToken)
    {
        var incomingIds = details
            .Where((m) => m.Id is not null)
            .Select((m) => m.Id)
            .Distinct()
            .ToList();

        var detailsForModification = new List<ProductDetailEntity>(incomingIds.Count);

        if (incomingIds.Any())
        {
            var spec = new SpecificationBuilder<ProductDetailEntity>()
                .AddCriteria((detail) => incomingIds.Contains(detail.Id))
                .Build();

            detailsForModification.AddRange(await productDetailRepository.GetAllBySpecAsync(spec, cancellationToken));

            if (incomingIds.Count != detailsForModification.Count)
            {
                throw new BadRequestException("One of the details does not exist.");
            }
        }

        return detailsForModification;
    }

    private static void ValidatePrice(PriceModel model, InventoryItemEntity item)
    {
        if (!model.Price.HasValue &&
            model.IsPromoted.HasValue && model.IsPromoted.Value &&
            (item.Price <= model.PromotionPrice))
        {
            throw new BadRequestException("Promotion price must be smaller than the actual price.");
        }
    }

    private async Task ValidateGeneralInfoAsync(GeneralInfoModel model, int shopId, CancellationToken cancellationToken)
    {
        if (model.BrandId.HasValue)
        {
            var brandSpec = new GetBrandByIdAndShopIdSpecification(model.BrandId.Value, shopId);

            if (!await brandRepository.ExistsAsync(brandSpec, cancellationToken))
            {
                throw new NotFoundException("The brand does not exist.");
            }
        }

        if (model.CategoryId.HasValue)
        {
            var categorySpec = new GetCategoryByIdAndShopIdSpecification(model.CategoryId.Value, shopId);

            if (!await categoryRepository.ExistsAsync(categorySpec, cancellationToken))
            {
                throw new NotFoundException("The category does not exist.");
            }
        }
    }
}