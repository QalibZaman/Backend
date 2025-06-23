using FluentValidation;
using NettoRetail.Application.Models.Feedback;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackModel>
{
  public CreateFeedbackValidator()
  {
    RuleFor(x => x.Rating)
        .InclusiveBetween(1, 5)
        .WithMessage("Rating must be between 1 and 5.");

    RuleFor(x => x.Title)
        .MaximumLength(60)
        .WithMessage("Title cannot be longer than 60 characters.");

    RuleFor(x => x.Text)
        .MaximumLength(500)
        .WithMessage("Feedback text cannot be longer than 500 characters.");
  }
}