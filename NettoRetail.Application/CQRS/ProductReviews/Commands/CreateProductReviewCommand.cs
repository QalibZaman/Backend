using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Services.IProductReviewService;

namespace NettoRetail.Application.CQRS.ProductReviews
{
    public record CreateProductReviewCommand(CreateProductReviewRequest Request, Guid CustomerId, Guid inventoryItemId) : IRequest;

    public class CreateProductReviewCommandHandler(IProductReviewService service) : IRequestHandler<CreateProductReviewCommand>
    {
        public async Task Handle(CreateProductReviewCommand command, CancellationToken cancellationToken)
        {
            await service.CreateProductReviewAsync(command.Request, command.CustomerId, command.inventoryItemId, cancellationToken);
        }
    }
}