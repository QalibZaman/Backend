using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace NettoRetail.Infrastructure.Services.IImageValidatorService;
public class ImageValidatorService : IImageValidatorService
{
    private const int MaxSizeMb = 5;
    private const int MinSizeMb = 1;
    private const int MinWidth = 800;
    private const int MinHeight = 800;
    private const int MaxWidth = 2000;
    private const int MaxHeight = 2000;
    private static string[] _allowedExtensions = [".jpg", ".jpeg", ".png"];

    public async Task ValidateImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(ext))
            throw new ValidationException($"Usupported file format: {ext}");

        if (file.Length < MinSizeMb * 1024 * 1024 || file.Length > MaxSizeMb * 1024 * 1024)
            throw new ValidationException("File size must be between 1MB and 5MB");

        try
        {
            using var image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken);
            if (image.Width < MinWidth || image.Height < MinHeight)
                throw new ValidationException($" Image is too small. Minimum size is {MinWidth}x{MinHeight}");

            if (image.Width > MaxWidth || image.Height > MaxHeight)
                throw new ValidationException($"Image is too large. Maximus size is {MaxWidth}x{MaxHeight}");
        }
        catch (UnknownImageFormatException)
        {
            throw new ValidationException("File is not a valid image");
        }
    }
}