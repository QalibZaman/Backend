using FluentValidation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class StaffCsvListValidator : AbstractValidator<List<StaffCsvModel>>
{
    private readonly IValidator<StaffCsvModel> _rowValidator;

    public StaffCsvListValidator(IValidator<StaffCsvModel> rowValidator)
    {
        _rowValidator = rowValidator;

        RuleFor(x => x)
            .CustomAsync(ValidateRowsAsync);
    }

    private static string BuildStaffListErrorMessage(IEnumerable<string> errors)
        => $"{CsvUploadErrors.StaffListValidationFailed}\n{string.Join("\n", errors)}";

    private async Task ValidateRowsAsync(
        List<StaffCsvModel> items,
        ValidationContext<List<StaffCsvModel>> context,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<string>();

        var duplicateEmails = items
            .Select((item, index) => new { Email = item.Email?.Trim() ?? string.Empty, Row = index + 2 })
            .Where(item => !string.IsNullOrWhiteSpace(item.Email))
            .GroupBy(item => item.Email, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var duplicate in duplicateEmails)
        {
            var rows = string.Join(", ", duplicate.Select(item => item.Row));
            validationErrors.Add($"Duplicate email '{duplicate.Key}' at rows {rows}.");
        }

        foreach (var (item, index) in items.Select((x, i) => (x, i + 2)))
        {
            var result = await _rowValidator.ValidateAsync(item, cancellationToken);
            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(error => error.ErrorMessage));
                validationErrors.Add($"Row {index}: {messages}");
            }
        }

        if (validationErrors.Count > 0)
            throw new BadRequestException(BuildStaffListErrorMessage(validationErrors));
    }
}
