using Microsoft.AspNetCore.Http;

namespace NettoRetail.Infrastructure.Services.IImageValidatorService;

public interface IImageValidatorService
{
    Task ValidateBannerImageAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task ValidateProfileImageAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task ValidateImage(IFormFile file);
}