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

        RuleFor(x => x.CategoryIds)
            .NotNull()
            .WithMessage("Category selection is required.")
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one category must be selected.")
            .Must(ids => ids.Count <= 8)
            .WithMessage("Maximum 8 categories allowed. Please remove one to add another.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate categories are not allowed.")
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Category id must be greater than 0.");
    }
}
