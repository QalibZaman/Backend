using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditCategoriesSectionRequestValidator : AbstractValidator<EditCategoriesSectionRequest>
{
    public EditCategoriesSectionRequestValidator()
    {
        RuleFor(x => x.Name)
      .Cascade(CascadeMode.Stop)
      .NotEmpty().WithMessage("\"Name of the section\" cannot be empty.")
      .Length(2, 60).WithMessage("The name must be between 2 and 60 characters long.")
      .Matches(@"^[A-Za-z0-9][A-Za-z0-9 _\-\.,!?'""()]*[A-Za-z0-9]$")
          .WithMessage("The field contains invalid characters or spaces at the beginning/end.");
    }
}
