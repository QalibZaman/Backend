using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.ProductReview;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.ProductReviews.Queries;

public record GetProductReviewByIdQuery(Guid Id, Guid CustomerId) : IRequest<ProductReviewResponse>;
public class GetProductReviewByIdQueryHandler(IProductReviewRepository productReviewRepository, IMapper mapper) : IRequestHandler<GetProductReviewByIdQuery, ProductReviewResponse>
{
    public async Task<ProductReviewResponse> Handle(GetProductReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetProductReviewByIdAndShopIdSpecification(request.Id, request.CustomerId);
        var productReview = await productReviewRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Product review not found");

        var productReviewResponse = mapper.Map<ProductReviewResponse>(productReview);

        return productReviewResponse;
    }
}
