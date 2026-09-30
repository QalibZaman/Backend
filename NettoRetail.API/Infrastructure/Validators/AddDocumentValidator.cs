using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddDocumentValidator : AbstractValidator<AddShopDocumentRequest>
    {
        public AddDocumentValidator()
        {
            ConfigureRules();
        }

        public void ConfigureRules()
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
                    x.RefundPolicy is not null ||
                    x.ShippingPolicy is not null ||
                    x.TermsAndConditions is not null ||
                    x.PrivacyPolicy is not null)
                .WithMessage("At least one document must be provided.");
        }

        private void FileCheck(IFormFile? file, ValidationContext<AddShopDocumentRequest> context)
        {
            if (file is null)
                return;

            const long MaxFileSize = 20 * 1024 * 1024;

            var allowedExtensions = new HashSet<string>(
                [".pdf", ".docx", ".odt"],
                StringComparer.OrdinalIgnoreCase);

            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension) || file.Length > MaxFileSize)
            {
                throw new BadRequestException(ValidationErrorMessages.ShopDocument.InvalidFormatOrSize);
            }
        }
    }
}
