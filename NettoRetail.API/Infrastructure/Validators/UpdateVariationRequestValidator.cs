using FluentValidation;
using NettoRetail.Application.Models.Request.Variation;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateVariationRequestValidator : AbstractValidator<UpdateVariationRequest>
    {
        public UpdateVariationRequestValidator()
        {
            RuleFor(x => x.ProductUpc.ToString())
                .NotEmpty().WithMessage("Product UPC is required.")
                .Matches("^[0-9]+$").WithMessage("Product UPC must contain only numeric characters.")
                .Length(12).WithMessage("Product UPC must be exactly 12 digits long.");

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

            RuleFor(x => x.ProductDescription)
                .MaximumLength(500).WithMessage("Product description must be less than 500 characters long.")
                .Matches("^(|[^ ].*[^ ]$)").WithMessage("Product description must not have leading or trailing spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.ProductDescription));

            When(x => !x.ApplyImagesFromParent, () =>
            {
                RuleFor(x => x.NewImages)
                    .NotNull().WithMessage("New images are required when not inherited from parent.")
                    .Must(list => list!.Count > 0).WithMessage("At least one image must be provided.");
            });

            RuleFor(x => x.AvailableAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Available amount must be greater or equal to 0.");

            RuleFor(x => x.ReservedAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Reserved amount must be greater or equal to 0.");

            RuleFor(x => x.OnHandAmount)
                .GreaterThanOrEqualTo(0).WithMessage("On-hand amount must be greater or equal to 0.");

            When(x => !x.ApplyDetailsFromParent, () =>
            {
                RuleFor(x => x.CustomParameters)
                    .NotNull().WithMessage("Custom parameters are required when not inherited from parent.")
                    .Must(list => list!.Count > 0).WithMessage("Custom parameters cannot be empty.");
            });

            RuleFor(x => x.Status)
                 .NotEmpty().WithMessage("Status is required.")
                 .Must(s => new[] { "Available", "Committed", "Unavailable", "Incoming" }
                 .Contains(s))
                 .WithMessage("Status must be one of: Available, Committed, Unavailable, Incoming.");
        }
    }
}
