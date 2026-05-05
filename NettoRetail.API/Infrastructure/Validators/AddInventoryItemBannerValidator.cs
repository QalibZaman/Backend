using FluentValidation;
using NettoRetail.Application.Models.Request.ProductBanner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddInventoryItemBannerRequestValidator : AbstractValidator<AddInventoryItemBannerRequest>
    {
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MinFileSize = 1 * 1024 * 1024;
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const int MinWidth = 800;
        private const int MinHeight = 800;
        private const int MaxWidth = 2000;
        private const int MaxHeight = 2000;
        private const float MinDpi = 72f;
        private const int MaxFilesPerProduct = 8;

        public AddInventoryItemBannerRequestValidator()
        {
            RuleFor(x => x.InventoryItemId)
                .NotEmpty().WithMessage("Inventory item ID is required.");

            RuleFor(x => x.Files)
                .NotEmpty().WithMessage("At least one image is required.")
                .Must(files => files.Count <= MaxFilesPerProduct)
                .WithMessage($"You can upload a maximum of {MaxFilesPerProduct} images per product.");

            RuleForEach(x => x.Files).ChildRules(file =>
            {
                file.RuleFor(f => f.FileName)
                    .Must(fn => allowedExtensions.Contains(System.IO.Path.GetExtension(fn).ToLower()))
                    .WithMessage("Unsupported file format. Please upload a JPEG or PNG.");

                file.RuleFor(f => f.Length)
                    .InclusiveBetween(MinFileSize, MaxFileSize)
                    .WithMessage("File size must be between 1MB and 5MB.");

                file.RuleFor(f => f)
                    .Must(f => HasValidDimensions(f))
                    .WithMessage($"Image dimensions must be between {MinWidth}x{MinHeight} and {MaxWidth}x{MaxHeight} pixels.");

                file.RuleFor(f => f)
                    .Must(f => HasValidResolution(f))
                    .WithMessage($"Image resolution must be at least {MinDpi} DPI.");
            });
        }

        private bool HasValidDimensions(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var info = Image.Identify(stream);
                if (info == null) return false;

                return info.Width >= MinWidth &&
                       info.Height >= MinHeight &&
                       info.Width <= MaxWidth &&
                       info.Height <= MaxHeight;
            }
            catch
            {
                return false;
            }
        }

        private bool HasValidResolution(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var image = Image.Load<Rgba32>(stream);
                return image.Metadata.HorizontalResolution >= MinDpi &&
                       image.Metadata.VerticalResolution >= MinDpi;
            }
            catch
            {
                return false;
            }
        }
    }
}
