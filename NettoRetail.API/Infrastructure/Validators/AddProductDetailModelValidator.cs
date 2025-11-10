using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddProductDetailModelValidator : AbstractValidator<ProductDetailModel>
{
    public AddProductDetailModelValidator()
    {
        Include(new BaseProductDetailsModelValidator());
    }
}
