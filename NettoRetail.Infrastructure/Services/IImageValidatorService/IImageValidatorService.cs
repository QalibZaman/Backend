using Microsoft.AspNetCore.Http;

namespace NettoRetail.Infrastructure.Services.IImageValidatorService;
public interface IImageValidatorService
{
    Task ValidateBannerImageAsync(IFormFile file, CancellationToken cancellationToken);
    void ValidateImage(IFormFile file);
}