using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NettoRetail.Shared.Constants;
using SixLabors.ImageSharp;

namespace NettoRetail.Infrastructure.Services.IImageValidatorService;

public class ImageValidatorService : IImageValidatorService
{
    private const int MaxSizeKb = 5 * 1024;

    private const int MinBannerSizeKb = 500;
    private const int MinBannerWidth = 800;
    private const int MinBannerHeight = 800;
    private const int MaxBannerWidth = 2000;
    private const int MaxBannerHeight = 2000;

    private static readonly string[] _allowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png"
    ];

    public async Task ValidateBannerImageAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
            throw new ValidationException($"Unsupported file format: {extension}");

        if (file.Length < MinBannerSizeKb * 1024 ||
            file.Length > MaxSizeKb * 1024)
        {
            throw new ValidationException("File size must be between 500KB and 5MB");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            using var image = await Image.LoadAsync(
                stream,
                cancellationToken);

            if (image.Width < MinBannerWidth ||
                image.Height < MinBannerHeight)
            {
                throw new ValidationException(
                    $"Image is too small. Minimum size is {MinBannerWidth}x{MinBannerHeight}");
            }

            if (image.Width > MaxBannerWidth ||
                image.Height > MaxBannerHeight)
            {
                throw new ValidationException(
                    $"Image is too large. Maximum size is {MaxBannerWidth}x{MaxBannerHeight}");
            }
        }
        catch (UnknownImageFormatException)
        {
            throw new ValidationException("File is not a valid image");
        }
        catch (ImageFormatException)
        {
            throw new ValidationException("Image file is corrupted or invalid");
        }
    }

    public async Task ValidateProfileImageAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File is empty");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!ProfilePictureConstants.AllowedExtensions.Contains(extension))
            throw new ValidationException($"Unsupported file format: {extension}");

        if (file.Length > ProfilePictureConstants.MaxFileSize)
            throw new ValidationException("Maximum file size is 5MB");

        try
        {
            await using var stream = file.OpenReadStream();

            using var image = await Image.LoadAsync(
                stream,
                cancellationToken);

            if (image.Width < ProfilePictureConstants.MinWidth ||
                image.Height < ProfilePictureConstants.MinHeight)
            {
                throw new ValidationException(
                    $"Image is too small. Minimum size is " +
                    $"{ProfilePictureConstants.MinWidth}x{ProfilePictureConstants.MinHeight}");
            }
        }
        catch (UnknownImageFormatException)
        {
            throw new ValidationException("File is not a valid image");
        }
        catch (ImageFormatException)
        {
            throw new ValidationException("Image file is corrupted or invalid");
        }
    }

    public async Task ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File is empty");

        try
        {
            await Image.LoadAsync(file.OpenReadStream());
        }
        catch (UnknownImageFormatException)
        {
            throw new ValidationException("File is not a valid image");
        }
        catch (ImageFormatException)
        {
            throw new ValidationException("Image file is corrupted or invalid");
        }
    }
}