using Microsoft.AspNetCore.Http;
using NettoRetail.Domain.Entities;
namespace NettoRetail.Application.Services.VariantMediaService
{
    public interface IVariantMediaService
    {
        Task InheritImagesFromParentAsync(InventoryItemEntity variation, InventoryItemEntity parent, CancellationToken ct);

        Task ReplaceWithExistingImagesAsync(InventoryItemEntity variation, List<Guid> existingImageIds, CancellationToken ct);

        Task AddUploadedImagesAsync(InventoryItemEntity variation, List<IFormFile> newImages, string pathPrefix, CancellationToken ct);
    }
}
