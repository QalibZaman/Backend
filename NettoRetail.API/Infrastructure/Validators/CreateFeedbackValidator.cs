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
           .Matches(@"^(?!\s)[A-Za-z0-9!@#$%^&*()_\-+={}[\]:;""'<>?,./\\|`~ ]+(?<!\s)$")
              .WithMessage("In title allowed: Latin letters, digits, special characters. No leading/trailing spaces.")
           .MaximumLength(60)
              .WithMessage("Title cannot be longer than 60 characters.");

        RuleFor(x => x.Text)
          .Cascade(CascadeMode.Stop)
          .Matches(@"^(?!\s)[A-Za-z0-9!@#$%^&*()_\-+={}[\]:;""'<>?,./\\|`~ ]+(?<!\s)$")
              .WithMessage("In Text allowed: Latin letters, digits, special characters. No leading/trailing spaces.")
          .MaximumLength(500)
              .WithMessage("Text cannot be longer than 500 characters.");
    }
}