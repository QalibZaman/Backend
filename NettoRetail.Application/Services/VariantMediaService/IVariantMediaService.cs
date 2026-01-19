using Microsoft.AspNetCore.Http;
using NettoRetail.Domain.Entities;
namespace NettoRetail.Application.Services.VariantMediaService
{
    public interface IVariantMediaService
    {
        Task InheritImagesFromParentAsync(InventoryItemEntity variation, InventoryItemEntity parent, CancellationToken cancellationToken);

        Task ReplaceWithExistingImagesAsync(InventoryItemEntity variation, List<Guid> existingImageIds, CancellationToken cancellationToken);

        Task AddUploadedImagesAsync(InventoryItemEntity variation, List<IFormFile> newImages, string pathPrefix, CancellationToken cancellationToken);
    }
}
