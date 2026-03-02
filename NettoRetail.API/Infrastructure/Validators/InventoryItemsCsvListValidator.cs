using FluentValidation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class InventoryItemsCsvListValidator : AbstractValidator<List<InventoryItemCsvModel>>
{
    private readonly IValidator<InventoryItemCsvModel> _rowValidator;

    public InventoryItemsCsvListValidator(IValidator<InventoryItemCsvModel> rowValidator)
    {
        _rowValidator = rowValidator;

        RuleFor(x => x)
            .CustomAsync(ValidateRowsAsync);
    }

    private async Task ValidateRowsAsync(List<InventoryItemCsvModel> items, ValidationContext<List<InventoryItemCsvModel>> context, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<string>();

        var duplicateRows = items
            .Select((item, index) => new { item.ProductUpc, Row = index + 2 })
            .GroupBy(x => x.ProductUpc)
            .Where(group => group.Count() > 1);

        foreach (var duplicate in duplicateRows)
        {
            var rows = string.Join(", ", duplicate.Select(x => x.Row));
            validationErrors.Add($"Duplicate Product UPC {duplicate.Key} at rows {rows}.");
        }

        foreach (var (item, index) in items.Select((x, i) => (x, i + 2)))
        {
            var result = await _rowValidator.ValidateAsync(item, cancellationToken);
            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                validationErrors.Add($"Row {index}: {messages}");
            }
        }

        if (validationErrors.Any())
            throw new BadRequestException(BuildItemsListErrorMessage(validationErrors));
    }

    private static string BuildItemsListErrorMessage(IEnumerable<string> errors)
        => $"{CsvUploadErrors.ItemsListValidationFailed}\n{string.Join("\n", errors)}";
}
