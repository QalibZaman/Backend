using Microsoft.AspNetCore.Http;

namespace NettoRetail.Infrastructure.Services.IImageValidatorService;
public interface IImageValidatorService
{
    Task ValidateImageAsync(IFormFile file, CancellationToken cancellationToken);
}