using FluentValidation;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Shared.Options;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditCategoriesSectionRequestValidator : AbstractValidator<EditCategoriesSectionRequest>
{
    public EditCategoriesSectionRequestValidator(IOptions<CategoriesSectionOptions> sectionOptions)
    {
        var options = sectionOptions.Value;

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("\"Name of the section\" cannot be empty.")
            .Length(2, 60).WithMessage("The name must be between 2 and 60 characters long.")
            .Matches(@"^[A-Za-z0-9][A-Za-z0-9 _\-\.,!?'""()]*[A-Za-z0-9]$")
            .WithMessage("The field contains invalid characters or spaces at the beginning/end.");

        RuleFor(x => x.CategoryIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Category selection is required.")
            .Must(ids => ids.Count > 0)
            .WithMessage(options.AtLeastOneCategoryErrorMessage)
            .Must(ids => ids.Count <= options.MaxSelectionCountValue)
            .WithMessage(options.MaxSelectionErrorMessage)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage(options.DuplicateCategoriesErrorMessage)
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Category id must be greater than 0.");
    }
}
