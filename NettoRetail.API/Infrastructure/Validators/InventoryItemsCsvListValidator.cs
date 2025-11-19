using FluentValidation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.CsvFileModel;

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

        foreach (var (item, index) in items.Select((x, i) => (x, i + 1)))
        {
            var result = await _rowValidator.ValidateAsync(item, cancellationToken);
            if (!result.IsValid)
            {
                var messages = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                validationErrors.Add($"Row {index}: {messages}");
            }
        }

        if (validationErrors.Any())
        {
            throw new BadRequestException($"Validation failed:\n{string.Join("\n", validationErrors)}");
        }
    }
}
