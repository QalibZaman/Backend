using FluentValidation;
using NettoRetail.Application.Models.Request;
using OtpNet;

namespace NettoRetail.API.Infrastructure
{
    public class CreateProductReviewRequestValidator : AbstractValidator<CreateProductReviewRequest>
    {
        public CreateProductReviewRequestValidator()
        {
            RuleFor(x => x.Rating)
                .NotEmpty().WithMessage("Rating is required.")
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

            RuleFor(x => x.ReviewText)
                .MaximumLength(5000).WithMessage("Review text must not exceed 5000 characters.")
                .When(x => x.ReviewText != null);
        }
    }
}
