using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Payments.Queries;

public record GetPaymentHistoryQuery(
    PaginationRequest PageModel,
    Guid AccountId,
    bool SortDescending)
    : IRequest<PaginatedResponse<PaymentHistoryResponse>>;

public class GetPaymentHistoryQueryHandler(
    IPaymentHistoryRepository paymentHistoryRepository,
    IMapper mapper)
    : IRequestHandler<GetPaymentHistoryQuery, PaginatedResponse<PaymentHistoryResponse>>
{
    public async Task<PaginatedResponse<PaymentHistoryResponse>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(request.PageModel.PageNumber, request.PageModel.PageSize);
        var take = request.PageModel.PageSize;

        var spec = new GetPaymentHistoriesByUserIdSpecification(request.AccountId, request.SortDescending, skip, take);

        var (items, totalCount) = await paymentHistoryRepository.GetAllPaginatedAsync(spec, cancellationToken);

        return new PaginatedResponse<PaymentHistoryResponse>
        {
            Items = mapper.Map<ICollection<PaymentHistoryResponse>>(items),
            TotalAmount = totalCount
        };
    }
}
