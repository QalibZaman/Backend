using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.ShopDocument;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.API.Infrastructure.Validators.ShopDocuments
{
    public class AddShopDocumentValidator : ShopDocumentValidator<AddShopDocumentRequest>
    {
    }
}
