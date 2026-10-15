using FluentValidation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Constants;

public abstract class ShopDocumentValidator<T>
    : AbstractValidator<T>
    where T : IShopDocumentRequest
{
    protected ShopDocumentValidator()
    {
        RuleFor(x => x.PrivacyPolicy)
            .Custom(FileCheck);

        RuleFor(x => x.RefundPolicy)
            .Custom(FileCheck);

        RuleFor(x => x.ShippingPolicy)
            .Custom(FileCheck);

        RuleFor(x => x.TermsAndConditions)
            .Custom(FileCheck);

        RuleFor(x => x)
            .Must(x =>
                x.PrivacyPolicy is not null ||
                x.RefundPolicy is not null ||
                x.ShippingPolicy is not null ||
                x.TermsAndConditions is not null)
            .WithMessage("At least one document must be provided.");
    }

    private void FileCheck(
        IFormFile? file,
        ValidationContext<T> context)
    {
        if (file is null)
            return;

        const long maxFileSize = 20 * 1024 * 1024;

        var allowedExtensions = new HashSet<string>(
            [".pdf", ".docx", ".odt"],
            StringComparer.OrdinalIgnoreCase);

        var extension = Path.GetExtension(file.FileName);

        if (!allowedExtensions.Contains(extension) ||
            file.Length > maxFileSize)
        {
            throw new BadRequestException(
                ValidationErrorMessages.ShopDocument.InvalidFormatOrSize);
        }
    }
}