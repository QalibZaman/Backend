using NettoRetail.Application.Models.Request.Variation;

namespace NettoRetail.Application.Services.VariationService
{
    public interface IVariationService
    {
        Task UpdateVariationAsync(UpdateVariationRequest model, int shopId, CancellationToken cancellationToken);
    }
}
