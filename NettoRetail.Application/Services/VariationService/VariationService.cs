using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Globalization;

namespace NettoRetail.Application.Services.VariationService
{
    public class VariationService(
        IInventoryItemRepository inventoryItemRepository,
        IProductDetailRepository detailRepository,
        IImageUploadService imageUploadService,
        IImageValidatorService imageValidatorService)
        : IVariationService
    {
        public async Task UpdateVariationAsync(UpdateVariationRequest model, int shopId, CancellationToken cancellationToken)
        {
            var variation = await LoadVariationAsync(model, shopId, cancellationToken);
            var parent = await LoadParentAsync(variation, shopId, cancellationToken);

            await ValidateUpcAsync(variation, model, cancellationToken);
            UpdateDescription(variation, parent, model);
            UpdatePrice(variation, model);

            if (model.ApplyImagesFromParent)
            {
                InheritImagesFromParent(variation, parent);
            }
            else
            {
                var hasExisting = model.ImageIds?.Any() == true;
                var hasUploads = model.NewImages?.Any() == true;

                if (hasExisting)
                    FilterExistingImages(variation, model.ImageIds!);

                if (hasUploads)
                {
                    var prefix = $"shops/{shopId}/products/{variation.Id}";
                    await AddUploadedImagesAsync(variation, model.NewImages!, prefix, cancellationToken);
                }
            }

            await UpdateDetailsAsync(variation, parent, model, cancellationToken);

            UpdateStatusAndAmounts(variation, model);

            variation.ProductUpc = model.ProductUpc;
            variation.IsActive = model.IsActive;

            await inventoryItemRepository.UpdateAsync(variation, cancellationToken);
        }

        private void InheritImagesFromParent(
            InventoryItemEntity variation,
            InventoryItemEntity parent)
        {
            variation.Images.Clear();

            foreach (var parentImage in parent.Images)
            {
                variation.Images.Add(new InventoryItemImageEntity
                {
                    InventoryItemId = variation.Id,
                    BlobUrl = parentImage.BlobUrl,
                    Order = parentImage.Order
                });
            }
        }

        private void FilterExistingImages(
            InventoryItemEntity variation,
            List<Guid> imageIds)
        {
            var existingIds = variation.Images.Select(x => x.Id).ToList();
            var invalidIds = imageIds.Except(existingIds).ToList();

            if (invalidIds.Any())
            {
                throw new BadRequestException($"The following Image IDs do not belong to this product: {string.Join(", ", invalidIds)}");
            }

            var toRemove = variation.Images
                .Where(img => !imageIds.Contains(img.Id))
                .ToList();

            foreach (var img in toRemove)
            {
                variation.Images.Remove(img);
            }
        }

        private async Task AddUploadedImagesAsync(
            InventoryItemEntity variation,
            List<IFormFile> newImages,
            string pathPrefix,
            CancellationToken cancellationToken)
        {
            foreach (var file in newImages)
            {
                await imageValidatorService.ValidateBannerImageAsync(file, cancellationToken);

                var blobUrl = await imageUploadService.UploadImageAsync(file, pathPrefix, cancellationToken);

                variation.Images.Add(new InventoryItemImageEntity
                {
                    InventoryItemId = variation.Id,
                    BlobUrl = blobUrl,
                    Order = variation.Images.Count
                });
            }
        }

        private void UpdateDescription(
            InventoryItemEntity variation,
            InventoryItemEntity parent,
            UpdateVariationRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                throw new BadRequestException("Description is required when not applying from parent.");
            if (model.ApplyDescriptionFromParent)
            {
                variation.Description = parent.Description;
                return;
            }
            else
            {
                variation.Description = model.Description.Trim();
            }
        }

        private void UpdatePrice(InventoryItemEntity variation, UpdateVariationRequest model)
        {
            variation.Price = model.Price;

            if (!model.ApplyPromotion)
            {
                variation.PromotionPrice = null;
                return;
            }

            if (model.PromotionPrice == null || model.PromotionPrice >= model.Price)
                throw new BadRequestException("Invalid promotion price.");

            variation.PromotionPrice = model.PromotionPrice.Value;
        }

        private async Task UpdateDetailsAsync(
             InventoryItemEntity variation,
             InventoryItemEntity parent,
             UpdateVariationRequest model,
             CancellationToken cancellationToken)
        {
            var deleteIds = variation.ProductDetails
                .Select(x => x.Id)
                .ToList();

            List<ProductDetailEntity> upsertedDetails;

            if (model.ApplyDetailsFromParent)
            {
                upsertedDetails = parent.ProductDetails
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new ProductDetailEntity
                    {
                        InventoryItemId = variation.Id,
                        Name = x.Name,
                        TypeEnum = x.TypeEnum,
                        Unit = x.Unit,
                        Value = x.Value,
                        OrderIndex = x.OrderIndex
                    })
                    .ToList();
            }
            else
            {
                if (model.CustomParameters == null || !model.CustomParameters.Any())
                    throw new BadRequestException("Custom parameters are required when not inheriting from parent.");

                upsertedDetails = model.CustomParameters
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => new ProductDetailEntity
                    {
                        InventoryItemId = variation.Id,
                        Name = x.Name,
                        TypeEnum = x.TypeEnum,
                        Unit = x.Unit,
                        Value = ConvertToString(x.Value),
                        OrderIndex = x.OrderIndex
                    })
                    .ToList();
            }

            await detailRepository.UpdateDetailsAsync(
                upsertedDetails,
                cancellationToken);
        }

        private string ConvertToString(object value)
        {
            return value switch
            {
                string s => s,
                bool b => b.ToString().ToLower(),
                int i => i.ToString(),
                decimal d => d.ToString(CultureInfo.InvariantCulture),
                double dbl => dbl.ToString(CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? ""
            };
        }

        private void UpdateStatusAndAmounts(InventoryItemEntity variation, UpdateVariationRequest model)
        {
            if (!Enum.TryParse<InventoryItemStatusEnum>(model.Status, true, out var status))
                throw new BadRequestException($"Invalid status: '{model.Status}'.");

            variation.Status = status;

            if (status is InventoryItemStatusEnum.Available or InventoryItemStatusEnum.Incoming)
            {
                variation.AvailableAmount = model.AvailableAmount;
                variation.ReservedAmount = model.ReservedAmount;
                variation.OnHandAmount = model.OnHandAmount;
            }
        }

        private async Task<InventoryItemEntity> LoadVariationAsync(UpdateVariationRequest model, int shopId, CancellationToken cancellationToken)
        {
            var spec = new GetInventoryItemWithImagesAndDetailsSpecification(model.InventoryItemId, shopId);

            var variation = await inventoryItemRepository.GetAsync(spec, cancellationToken)
                ?? throw new NotFoundException("Variation does not exist.");

            if (!variation.IsVariation)
                throw new BadRequestException("This item is not a variation.");

            return variation;
        }

        private async Task<InventoryItemEntity> LoadParentAsync(InventoryItemEntity variation, int shopId, CancellationToken cancellationToken)
        {
            if (variation.ParentItemId is null)
                throw new BadRequestException("Variation has no parent product.");

            var spec = new GetInventoryItemWithImagesAndDetailsSpecification(variation.ParentItemId.Value, shopId);

            return await inventoryItemRepository.GetAsync(spec, cancellationToken)
                ?? throw new NotFoundException("Parent product not found.");
        }

        private async Task ValidateUpcAsync(
            InventoryItemEntity variation,
            UpdateVariationRequest model,
            CancellationToken cancellationToken)
        {
            if (variation.ProductUpc == model.ProductUpc)
                return;

            var spec = new GetProductByUpcSpecification(model.ProductUpc);

            if (await inventoryItemRepository.Exists(spec, cancellationToken))
                throw new ConflictException("This product UPC is already registered.");
        }
    }
}
