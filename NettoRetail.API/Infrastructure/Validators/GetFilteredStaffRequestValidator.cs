using FluentValidation;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.Application.Validators.Staff;

public class GetStaffListQueryValidator : AbstractValidator<GetStaffListQuery>
{
    public GetStaffListQueryValidator()
    {
        RuleFor(x => x.Model.FirstName)
            .Must(BeValidName)
            .When(x => !string.IsNullOrWhiteSpace(x.Model.FirstName))
            .WithMessage("Invalid First/Last Name. Please check format or length.");

        RuleFor(x => x.Model.LastName)
            .Must(BeValidName)
            .When(x => !string.IsNullOrWhiteSpace(x.Model.LastName))
            .WithMessage("Invalid First/Last Name. Please check format or length.");

        RuleForEach(x => x.Model.SortingOptions)
            .ChildRules(opt =>
            {
                opt.RuleFor(o => o.Field)
                    .NotEmpty()
                    .WithMessage("Invalid sorting field. Allowed options: FirstName, LastName, Email, Role, Status.")
                    .Must(BeValidSortingField)
                    .WithMessage("Invalid sorting field. Allowed options: FirstName, LastName, Email, Role, Status.");

                opt.RuleFor(o => o.IsDescending)
                    .NotNull()
                    .WithMessage("SortingOptions.isDescending is required.");
            });

        RuleFor(x => x.Model.SortingOptions)
            .Must(NoDuplicateSortingFields)
            .When(x => x.Model.SortingOptions != null && x.Model.SortingOptions.Any())
            .WithMessage("Duplicate sorting fields are not allowed.");
    }

    private static bool BeValidName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value != value.Trim()) return false;
        if (value.Length < 2) return false;
        return value.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z');
    }

    private static bool BeValidSortingField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field)) return false;
        if (field != field.Trim()) return false;
        if (!field.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')) return false;

        var f = field.ToLower();
        return f == "firstname" || f == "lastname" || f == "email" || f == "role" || f == "status";
    }

    private static bool NoDuplicateSortingFields(List<SortingOption>? options)
    {
        if (options == null || options.Count == 0) return true;

        var normalized = options
            .Where(o => !string.IsNullOrWhiteSpace(o.Field))
            .Select(o => o.Field!.Trim().ToLower())
            .ToList();

        return normalized.Distinct().Count() == normalized.Count;
    }
}
