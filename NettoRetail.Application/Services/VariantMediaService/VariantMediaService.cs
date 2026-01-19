using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Services.ImageUploadService;
using NettoRetail.Domain.Entities;
namespace NettoRetail.Application.Services.VariantMediaService
{
    public class VariantMediaService : IVariantMediaService
    {
        private readonly IImageUploadService _imageUploadService;

        public VariantMediaService(IImageUploadService imageUploadService)
        {
            _imageUploadService = imageUploadService;
        }

        public Task InheritImagesFromParentAsync(
            InventoryItemEntity variation,
            InventoryItemEntity parent,
            CancellationToken cancellationToken)
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

            return Task.CompletedTask;
        }

        public Task ReplaceWithExistingImagesAsync(
            InventoryItemEntity variation,
            List<Guid> existingImageIds,
            CancellationToken cancellationToken)
        {
            variation.Images.Clear();

            var existing = variation.Images
                .Where(x => existingImageIds.Contains(x.Id))
                .ToList();

            foreach (var img in existing)
            {
                variation.Images.Add(new InventoryItemImageEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    BlobUrl = img.BlobUrl,
                    Order = img.Order
                });
            }

            return Task.CompletedTask;
        }

        public async Task AddUploadedImagesAsync(
            InventoryItemEntity variation,
            List<IFormFile> newImages,
            string pathPrefix,
            CancellationToken cancellationToken)
        {
            foreach (var file in newImages)
            {
                var blobUrl = await _imageUploadService.UploadImageAsync(file, pathPrefix, cancellationToken);

                variation.Images.Add(new InventoryItemImageEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    BlobUrl = blobUrl,
                    Order = variation.Images.Count,
                });
            }
        }
    }
}
