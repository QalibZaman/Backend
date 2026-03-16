using FluentValidation;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class SearchFieldModelValidator : AbstractValidator<SearchFieldModel>
{
    private static readonly Dictionary<string, string> FieldTypes = new()
    {
        ["creator"] = "text",
        ["customerfirstname"] = "text",
        ["customerlastname"] = "text",
        ["customeremail"] = "email",
        ["orderid"] = "numeric",
        ["customerid"] = "numeric"
    };

    public SearchFieldModelValidator()
    {
        RuleFor(x => x.Field)
            .NotEmpty()
            .Must(f => FieldTypes.ContainsKey(f.ToLower()))
            .WithMessage(ValidationErrorMessages.Search.InvalidSearchField);

        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage(ValidationErrorMessages.Search.SearchTermRequired)
            .MaximumLength(100).WithMessage(ValidationErrorMessages.Search.SearchTermMaxLength)
            .DependentRules(() =>
            {
                When(x => IsTextField(x.Field), () =>
                {
                    RuleFor(x => x.SearchTerm)
                        .Matches(@"^[\p{L}\p{P}\p{N}\s-]*$")
                        .WithMessage(ValidationErrorMessages.Search.InvalidCharacters);
                });

                When(x => IsNumericField(x.Field), () =>
                {
                    RuleFor(x => x.SearchTerm)
                        .Matches(@"^\d+$")
                        .WithMessage(ValidationErrorMessages.Search.InvalidNumericValue);
                });

                When(x => IsEmailField(x.Field), () =>
                {
                    RuleFor(x => x.SearchTerm)
                        .EmailAddress().WithMessage(ValidationErrorMessages.Search.InvalidEmailFormat);
                });
            });
    }

    private bool IsTextField(string? field) =>
        !string.IsNullOrEmpty(field) && FieldTypes.TryGetValue(field.ToLower(), out var type) && type == "text";

    private bool IsNumericField(string? field) =>
        !string.IsNullOrEmpty(field) && FieldTypes.TryGetValue(field.ToLower(), out var type) && type == "numeric";

    private bool IsEmailField(string? field) =>
        !string.IsNullOrEmpty(field) && FieldTypes.TryGetValue(field.ToLower(), out var type) && type == "email";
}