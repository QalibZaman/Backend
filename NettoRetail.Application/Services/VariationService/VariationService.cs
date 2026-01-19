using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.VariantMediaService;
using NettoRetail.Application.Services.VariationService.VariationDetailsService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.VariationService
{
    public class VariationService(
        IInventoryItemRepository inventoryItemRepository,
        IVariantMediaService mediaService,
        IVariationDetailsService detailsService)
        : IVariationService
    {
        public async Task UpdateVariationAsync(UpdateVariationRequest model, int shopId, CancellationToken ct)
        {
            var variation = await LoadVariationAsync(model, shopId, ct);
            var parent = await LoadParentAsync(variation, shopId, ct);

            if (variation.ProductUpc != model.ProductUpc)
            {
                var upcSpec = new GetProductByUpcSpecification(model.ProductUpc);
                if (await inventoryItemRepository.Exists(upcSpec, ct))
                    throw new ConflictException("This product UPC is already registered.");
            }

            if (model.ApplyDescriptionFromParent)
            {
                variation.Description = parent.Description;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Description))
                    throw new BadRequestException("Description is required when not applying from parent.");

                variation.Description = model.Description.Trim();
            }

            variation.Price = model.Price;
            if (model.ApplyPromotion)
            {
                if (model.PromotionPrice == null || model.PromotionPrice >= model.Price)
                    throw new BadRequestException("Invalid promotion price.");

                variation.PromotionPrice = model.PromotionPrice;
            }
            else
            {
                variation.PromotionPrice = null;
            }

            if (model.ApplyImagesFromParent)
            {
                await mediaService.InheritImagesFromParentAsync(variation, parent, ct);
            }
            else
            {
                if (model.ImageIds?.Any() == true)
                    await mediaService.ReplaceWithExistingImagesAsync(variation, model.ImageIds, ct);

                if (model.NewImages?.Any() == true)
                {
                    var pathPrefix = $"shops/{shopId}/products/{variation.Id}";
                    await mediaService.AddUploadedImagesAsync(variation, model.NewImages, pathPrefix, ct);
                }
            }

            if (!Enum.TryParse<InventoryItemStatusEnum>(model.Status, true, out var status))
                throw new BadRequestException($"Invalid status: '{model.Status}'.");

            variation.Status = status;

            var editableStatuses = new[]
            {
                InventoryItemStatusEnum.Available,
                InventoryItemStatusEnum.Incoming
            };

            if (editableStatuses.Contains(status))
            {
                variation.AvailableAmount = model.AvailableAmount;
                variation.ReservedAmount = model.ReservedAmount;
                variation.OnHandAmount = model.OnHandAmount;
            }

            if (model.ApplyDetailsFromParent)
            {
                await detailsService.InheritDetailsAsync(variation, parent, ct);
            }
            else
            {
                if (model.CustomParameters == null || !model.CustomParameters.Any())
                    throw new BadRequestException("Custom parameters are required when not inheriting from parent.");

                await detailsService.ApplyCustomDetailsAsync(variation, model.CustomParameters, ct);
            }

            variation.ProductUpc = model.ProductUpc;
            variation.IsActive = model.IsActive;

            await inventoryItemRepository.UpdateAsync(variation, ct);
        }

        private async Task<InventoryItemEntity> LoadVariationAsync(UpdateVariationRequest model, int shopId, CancellationToken ct)
        {
            var spec = new GetInventoryItemWithImagesAndDetailsSpecification(model.InventoryItemId, shopId);

            var variation = await inventoryItemRepository.GetAsync(spec, ct)
                ?? throw new NotFoundException("Variation does not exist.");

            if (!variation.IsVariation)
                throw new BadRequestException("This item is not a variation.");

            return variation;
        }

        private async Task<InventoryItemEntity> LoadParentAsync(InventoryItemEntity variation, int shopId, CancellationToken ct)
        {
            if (variation.ParentItemId is null)
                throw new BadRequestException("Variation has no parent product.");

            var spec = new GetInventoryItemWithImagesAndDetailsSpecification(variation.ParentItemId.Value, shopId);

            return await inventoryItemRepository.GetAsync(spec, ct)
                ?? throw new NotFoundException("Parent product not found.");
        }
    }
}
