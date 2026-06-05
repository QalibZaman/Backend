using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Infrastructure.Interfaces;

namespace NettoRetail.Application.CQRS.ProductReviews.Queries;

public record GetProductReviewImageQuery(string ProductReviewImage, Guid CustomerId) : IRequest<string>;
public class GetProductReviewImageQueryHandler(IAzureBlobService azureBlobService) : IRequestHandler<GetProductReviewImageQuery, string>
{
    public async Task<string> Handle(GetProductReviewImageQuery request, CancellationToken cancellationToken)
    {
        var sasUrl = azureBlobService.GenerateBlobSasUrlFromUrl(request.ProductReviewImage, BlobSasPermissions.Read);

        return sasUrl;
    }
}
