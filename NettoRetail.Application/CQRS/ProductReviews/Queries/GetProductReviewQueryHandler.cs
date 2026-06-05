using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.ProductReview;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.ProductReviews.Queries;

public record GetProductReviewQuery(Guid CustomerId, PaginationRequest Pagination) : IRequest<PaginatedResponse<ProductReviewResponse>>;
public class GetProductReviewQueryHandler(IProductReviewRepository productReviewRepository, IMapper mapper) : IRequestHandler<GetProductReviewQuery, PaginatedResponse<ProductReviewResponse>>
{
    public async Task<PaginatedResponse<ProductReviewResponse>> Handle(GetProductReviewQuery request, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(request.Pagination.PageNumber, request.Pagination.PageSize);
        var spec = new GetProductReviewsByShopIdSpecificationWithPagination(
            request.CustomerId,
            skip,
            request.Pagination.PageSize);

        var (items, totalCount) = await productReviewRepository.GetAllPaginatedAsync(spec, cancellationToken);
        var productReviews = mapper.Map<ICollection<ProductReviewResponse>>(items);

        return new PaginatedResponse<ProductReviewResponse>
        {
            Items = productReviews,
            TotalAmount = totalCount
        };
    }
}
