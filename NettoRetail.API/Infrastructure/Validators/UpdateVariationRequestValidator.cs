using FluentValidation;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateVariationRequestValidator : AbstractValidator<UpdateVariationRequest>
    {
        private static readonly string[] AllowedStatuses =
            Enum.GetNames<InventoryItemStatusEnum>();

        public UpdateVariationRequestValidator()
        {
            RuleFor(x => x.ProductUpc)
                .NotEmpty().WithMessage("Product UPC is required.")
                .Matches(@"^\d{12}$").WithMessage("Product UPC must be exactly 12 digits long.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0.")
                .PrecisionScale(18, 2, false).WithMessage("Price must not have more than 2 decimal places.");

            When(x => x.ApplyPromotion, () =>
            {
                RuleFor(x => x.PromotionPrice)
                    .NotNull().WithMessage("Promotion price is required when promotion is applied.")
                    .GreaterThanOrEqualTo(0).WithMessage("Promotion price must be greater or equal to 0.")
                    .PrecisionScale(18, 2, false).WithMessage("Promotion price must not have more than 2 decimal places.")
                    .LessThan(x => x.Price).WithMessage("Promotion price must be less than regular price.");
            });

            When(x => !x.ApplyDescriptionFromParent, () =>
            {
                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Description is required when not inherited from parent.")
                    .MaximumLength(500).WithMessage("Description must be less than 500 characters long.")
                    .Matches(@"^[A-Za-z0-9 ,.\-_'()]*$")
                        .WithMessage("Description must only contain letters, digits, spaces, and common punctuation.")
                    .Matches("^(|[^ ].*[^ ]$)")
                        .WithMessage("Description must not have leading or trailing spaces.");
            });

            When(x => !x.ApplyImagesFromParent, () =>
            {
                RuleFor(x => x)
                    .Must(x => (x.NewImages != null && x.NewImages.Count > 0) ||
                               (x.ImageIds != null && x.ImageIds.Count > 0))
                    .WithMessage("You must either upload new images or select existing images when not inheriting from parent.")
                    .WithName("Images");
            });

            RuleFor(x => x.AvailableAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Available amount must be greater or equal to 0.");

            RuleFor(x => x.ReservedAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Reserved amount must be greater or equal to 0.");

            RuleFor(x => x.OnHandAmount)
                .GreaterThanOrEqualTo(0).WithMessage("On-hand amount must be greater or equal to 0.");

            RuleFor(x => x)
                .Must(x => x.ReservedAmount <= x.AvailableAmount)
                .WithMessage("Reserved amount cannot exceed the available amount.");

            RuleFor(x => x)
                .Must(x => x.ReservedAmount + x.AvailableAmount <= x.OnHandAmount)
                .WithMessage("The sum of Reserved Amount and Available Amount should not exceed the On-hand Amount.");

            When(x => !x.ApplyDetailsFromParent, () =>
            {
                RuleFor(x => x.CustomParameters)
                    .Cascade(CascadeMode.Stop)
                    .NotNull().WithMessage("Custom parameters are required when not inherited from parent.")
                    .Must(list => list!.Count > 0).WithMessage("Custom parameters cannot be empty.");

                RuleForEach(x => x.CustomParameters!)
                    .SetValidator(new VariationCustomDetailModelValidator());
            });

            RuleFor(x => x.Status)
                 .NotEmpty().WithMessage("Status is required.")
                 .Must(s => AllowedStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
                 .WithMessage("Status must be one of: Available, Committed, Unavailable, Incoming.");
        }
    }
}
