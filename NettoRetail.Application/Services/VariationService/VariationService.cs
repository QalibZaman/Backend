using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Globalization;

namespace NettoRetail.Application.Services.VariationService
{
    public class VariationService(
        IInventoryItemRepository inventoryItemRepository,
        IImageUploadService imageUploadService)
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

            var hasExisting = model.ImageIds?.Any() == true;
            var hasUploads = model.NewImages?.Any() == true;

            if (hasExisting)
                ReplaceWithExistingImages(variation, model.ImageIds!);

            if (hasUploads)
            {
                var prefix = $"shops/{shopId}/products/{variation.Id}";
                await AddUploadedImagesAsync(variation, model.NewImages!, prefix, cancellationToken);
            }

            if (model.ApplyDetailsFromParent)
            {
                InheritDetails(variation, parent);
            }

            if (model.CustomParameters == null || !model.CustomParameters.Any())
                throw new BadRequestException("Custom parameters are required when not inheriting from parent.");
            ApplyCustomDetails(variation, model.CustomParameters);

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
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    BlobUrl = parentImage.BlobUrl,
                    Order = parentImage.Order
                });
            }
        }

        private void ReplaceWithExistingImages(
            InventoryItemEntity variation,
            List<Guid> existingImageIds)
        {
            var matched = variation.Images
                .Where(x => existingImageIds.Contains(x.Id))
                .ToList();

            variation.Images.Clear();

            foreach (var img in matched)
            {
                variation.Images.Add(new InventoryItemImageEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    BlobUrl = img.BlobUrl,
                    Order = img.Order
                });
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
                var blobUrl = await imageUploadService.UploadImageAsync(file, pathPrefix, cancellationToken);

                variation.Images.Add(new InventoryItemImageEntity
                {
                    Id = Guid.NewGuid(),
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
            if (model.ApplyDescriptionFromParent)
            {
                variation.Description = parent.Description;
                return;
            }

            if (string.IsNullOrWhiteSpace(model.Description))
                throw new BadRequestException("Description is required when not applying from parent.");

            variation.Description = model.Description.Trim();
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

        private void InheritDetails(
            InventoryItemEntity variation,
            InventoryItemEntity parent)
        {
            variation.ProductDetails.Clear();

            foreach (var parentDetail in parent.ProductDetails.OrderBy(x => x.OrderIndex))
            {
                variation.ProductDetails.Add(new ProductDetailEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    Name = parentDetail.Name,
                    TypeEnum = parentDetail.TypeEnum,
                    Unit = parentDetail.Unit,
                    Value = parentDetail.Value,
                    OrderIndex = parentDetail.OrderIndex
                });
            }
        }

        private void ApplyCustomDetails(
            InventoryItemEntity variation,
            List<CustomDetailModel> customDetails)
        {
            variation.ProductDetails.Clear();

            foreach (var detail in customDetails.OrderBy(x => x.OrderIndex))
            {
                variation.ProductDetails.Add(new ProductDetailEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    Name = detail.Name,
                    TypeEnum = detail.TypeEnum,
                    Unit = detail.Unit,
                    Value = ConvertToString(detail.Value),
                    OrderIndex = detail.OrderIndex
                });
            }
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
