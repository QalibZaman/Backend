using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.AdminProductReview;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.AdminProductReview;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.AdminProductReviews.Queries;

public record GetProductReviewsQuery(PaginationRequest Model, Guid InventoryItemId) : IRequest<PaginatedResponse<AdminProductReviewResponse>>;
public class GetProductReviewsQueryHandler(IProductReviewRepository productReviewRepository, IMapper mapper) : IRequestHandler<GetProductReviewsQuery, PaginatedResponse<AdminProductReviewResponse>>
{
   public async Task<PaginatedResponse<AdminProductReviewResponse>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
   {
      var skip = PaginationHelper.CalculateSkip(request.Model.PageNumber, request.Model.PageSize);
      var specs = new GetProductReviewsByInventoryItemIdSpecificationWithPagination(request.InventoryItemId, skip, request.Model.PageSize)
         .AddOrderByDescending(x => x.DateSubmitted);
      var (items, totalCount) = await productReviewRepository.GetAllPaginatedAsync(specs, cancellationToken);
      var productReviews = mapper.Map<ICollection<AdminProductReviewResponse>>(items);
      return new PaginatedResponse<AdminProductReviewResponse>()
      {
         Items = productReviews,
         TotalAmount = totalCount,
      };
   }
}