using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpsertProductDetailModelValidator : AbstractValidator<UpsertProductDetailModel>
{
    public UpsertProductDetailModelValidator()
    {
        Include(new ProductDetailsModelValidator());
    }
}
