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
            CancellationToken ct)
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
            CancellationToken ct)
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
            CancellationToken ct)
        {
            foreach (var file in newImages)
            {
                var blobUrl = await _imageUploadService.UploadImageAsync(file, pathPrefix, ct);

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
