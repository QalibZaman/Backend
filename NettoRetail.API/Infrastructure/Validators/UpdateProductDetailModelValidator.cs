using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateProductDetailModelValidator : AbstractValidator<UpdateProductDetailModel>
{
    public UpdateProductDetailModelValidator()
    {
        Include(new BaseProductDetailsModelValidator());
    }
}
