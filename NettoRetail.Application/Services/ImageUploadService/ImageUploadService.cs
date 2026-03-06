using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;

namespace NettoRetail.Application.Services.ImageUploadService
{
    public class ImageUploadService(
        IAzureBlobService azureBlobService,
        IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
        IImageValidatorService imageValidatorService) : IImageUploadService
    {
        public async Task<string> UploadImageAsync(
            IFormFile file,
            string pathPrefix,
            CancellationToken cancellationToken)
        {
            if (file == null)
                throw new ArgumentException("No image provided");

            var fileName = $"{pathPrefix}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var container = azureBlobStorageOptions.Value.ContainerName;
            var category = Shared.Enums.BlobContentCategoryEnum.Image;

            using var stream = file.OpenReadStream();

            string? blobUrl = null;

            try
            {
                blobUrl = await azureBlobService.UploadAsync(fileName, stream, container, category);
                return blobUrl;
            }
            catch (Exception ex)
            {
                if (blobUrl != null)
                {
                    await azureBlobService.DeleteByUrlAsync(blobUrl);
                }

                throw new ApplicationException("Unable to upload image");
            }
        }
    }
}