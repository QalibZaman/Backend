using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICountryValidationService;

public interface ICountryDetailsValidationService
{
    Task ValidateAsync(ShopModel shop, CancellationToken cancellationToken);
}
