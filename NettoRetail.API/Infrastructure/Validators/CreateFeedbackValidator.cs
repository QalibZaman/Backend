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
     .Cascade(CascadeMode.Stop)
     .NotEmpty().WithMessage("Title is required.")
     .Must(title => !string.IsNullOrWhiteSpace(title) && title == title.Trim())
         .WithMessage("Title cannot contain leading or trailing spaces.")
     .MaximumLength(60)
         .WithMessage("Title cannot be longer than 60 characters.");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Text is required.")
            .Must(text => !string.IsNullOrWhiteSpace(text) && text == text.Trim())
                .WithMessage("Text cannot contain leading or trailing spaces.")
            .MaximumLength(500)
                .WithMessage("Text cannot be longer than 500 characters.");
    }
}