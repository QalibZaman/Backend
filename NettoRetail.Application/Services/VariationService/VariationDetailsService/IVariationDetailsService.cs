using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.VariationService.VariationDetailsService
{
    public interface IVariationDetailsService
    {
        Task InheritDetailsAsync(InventoryItemEntity variation, InventoryItemEntity parent, CancellationToken ct);
        Task ApplyCustomDetailsAsync(InventoryItemEntity variation, List<CustomDetailModel> customDetails, CancellationToken ct);
    }
}
