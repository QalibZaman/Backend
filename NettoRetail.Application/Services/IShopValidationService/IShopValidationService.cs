
using NettoRetail.Application.Models.Shop;

namespace NettoRetail.Application.Services.IShopValidationService;

public interface IShopValidationService
{
    Task ValidateShopOnCreate(ShopValidationModel model, CancellationToken cancellationToken);
    Task ValidateShopOnUpdate(ShopValidationModel model, int shopId, CancellationToken cancellationToken);
}
