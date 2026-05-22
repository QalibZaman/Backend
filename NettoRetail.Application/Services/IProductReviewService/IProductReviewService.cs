using NettoRetail.Application.Models.Request;

namespace NettoRetail.Application.Services.IProductReviewService;

public interface IProductReviewService
{
    Task CreateProductReviewAsync(
        CreateProductReviewRequest request,
        Guid customerId,
        Guid inventoryItemId,
        CancellationToken cancellationToken
        );
}