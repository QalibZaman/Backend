using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Services.IProductReviewService;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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