using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddCategoryRequestValidator : CategoryUpsertRequestValidator<AddCategoryRequest>
{
}
