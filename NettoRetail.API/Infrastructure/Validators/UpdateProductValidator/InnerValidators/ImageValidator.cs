using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

// TODO: Extract it into another endpoint. Will be used in the next task.
public class ImageValidator : AbstractValidator<ImageModel>
{
    private const int MIN_SIZE_BYTES = 1 * 1024 * 1024;
    private const int MAX_SIZE_BYTES = 5 * 1024 * 1024;
    private const int MAX_DESCRIPTION_LENGTH = 125;
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png"];
    public ImageValidator()
    {
        RuleFor((model) => model)
            .Must(HasAnyPropToUpdate)
            .WithMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);

        When((model) => model.Image is not null, () =>
        {
            RuleFor((model) => model.Image)
                .Must((image) => AllowedTypes.Contains(image!.ContentType))
                .WithMessage($"Only these image types are allowed: {string.Join(", ", AllowedTypes)}.");

            RuleFor((model) => model.Image)
                .Must((image) => image!.Length >= MIN_SIZE_BYTES && image.Length <= MAX_SIZE_BYTES)
                .WithMessage($"The size of the image must be between {ToReadableMegabytes(MIN_SIZE_BYTES)}MB and {ToReadableMegabytes(MAX_SIZE_BYTES)}MB.");
        });

        When((model) => model.Description is not null, () =>
        {
            RuleFor((model) => model.Description)
                .Must((description) => description!.Trim().Length < MAX_DESCRIPTION_LENGTH)
                .WithMessage($"Description must be {MAX_DESCRIPTION_LENGTH} characters at max.");
        });
    }

    private static bool HasAnyPropToUpdate(ImageModel model)
    {
        return model.Image is not null ||
            model.Description is not null ||
            model.Order is not null;
    }

    private static string ToReadableMegabytes(int bytes)
    {
        return (bytes / (1024 * 1024)).ToString();
    }
}