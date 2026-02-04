using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Services.ImageUploadService
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(
            IFormFile file,
            string pathPrefix,
            CancellationToken cancellationToken);
    }
}
